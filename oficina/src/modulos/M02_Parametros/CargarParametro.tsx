import { useState, type ReactElement } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, Info } from 'lucide-react';

import { Boton, Campo, Nota, Panel, Pastilla, Tabla, avisar } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
import { usarQuienEjecuta } from '../../app/puesto';

/**
 * `PT-099` — Carga de un parámetro normativo con vigencia y respaldo.
 *
 * ── Las tres cosas que esta pantalla no deja hacer ──────────────────────────
 * <b>1 · Cargar sin respaldo.</b> Un parámetro sin documento que lo sostenga no se puede
 * defender ante el Tribunal Superior de Cuentas: la cifra existe y nadie sabe de dónde salió.
 *
 * <b>2 · Solapar dos vigencias.</b> Dos valores rigiendo el mismo día vuelven indeterminado todo
 * cálculo de ese día, y el error no aparece al cargar sino meses después.
 *
 * <b>3 · Dejar un hueco.</b> El fallo silencioso: no rompe nada hoy, rompe cuando alguien liquida
 * una misión de esos días y no hay tabla con qué calcularla.
 *
 * ── Y la que sí deja, a propósito ───────────────────────────────────────────
 * <b>La vigencia puede arrancar en el pasado.</b> Tiene que poder: una tarifa publicada en
 * septiembre que rige desde agosto es lo normal, no la excepción, y `RN-46` manda calcular con
 * la tabla vigente <b>a la fecha del hecho</b>. Lo que no puede es pasar desapercibido — quien
 * aprueba lo ve declarado en `PT-100`.
 */
export default function CargarParametro(): ReactElement {
  const quienEjecuta = usarQuienEjecuta();
  const cliente = useQueryClient();

  const [clave, setClave] = useState('');
  const [valor, setValor] = useState('');
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');
  const [fuente, setFuente] = useState('');
  const [verificadoEl, setVerificadoEl] = useState('');
  const [archivo, setArchivo] = useState<File | null>(null);

  const { data: versiones, isError } = useQuery({
    queryKey: ['parametros', clave],
    queryFn: () => pedir<Version[]>(`/parametros?clave=${encodeURIComponent(clave)}`),
    enabled: clave.trim().length > 0,
  });

  const cargar = useMutation({
    mutationFn: async () => {
      // ⚠️ **Primero el documento, después la carga.** Si se cayera entre medias queda un
      // archivo sin parámetro: ocupa disco y no engaña a nadie. Al revés quedaría un
      // parámetro que promete un respaldo inexistente — que es exactamente el defecto que
      // `HU-145` destapó, y que la aprobación ahora bloquea.
      const idAdjunto = await subirRespaldo(archivo!);

      return pedir<{ id: string }>('/parametros', {
        method: 'POST',
        body: JSON.stringify({
          clave: clave.trim(),
          valor: valor.trim(),
          vigenteDesde: desde,
          vigenteHasta: hasta === '' ? null : hasta,
          respaldoAdjunto: idAdjunto,
          fuente: fuente.trim(),
          verificadoEl: verificadoEl === '' ? null : verificadoEl,
          cargadoPor: quienEjecuta,
          momento: new Date().toISOString(),
        }),
      });
    },
    onSuccess: async () => {
      // Lo que hay que decir acá es que **todavía no rige**. Un «guardado» a secas haría
      // creer que el valor ya se aplica, y el doble control quedaría de adorno.
      avisar.exito('Cargado y pendiente de aprobación. Todavía no se aplica en ningún cálculo.');
      setValor('');
      setDesde('');
      setHasta('');
      setFuente('');
      setVerificadoEl('');
      setArchivo(null);
      await cliente.invalidateQueries({ queryKey: ['parametros', clave] });
      await cliente.invalidateQueries({ queryKey: ['parametros-pendientes'] });
    },
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo cargar el parámetro.'),
  });

  const completo =
    clave.trim() !== '' && valor.trim() !== '' && desde !== '' &&
    fuente.trim() !== '' && verificadoEl !== '' && archivo !== null;

  // El retroactivo se declara **al cargar**, no sólo al aprobar: quien lo escribe puede
  // haberse equivocado de año, y es el único momento en que corregirlo no cuesta nada.
  const esRetroactivo = desde !== '' && desde < hoy();

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Cargar un parámetro normativo
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Lo que se carga acá <b>no rige hasta que otra persona lo apruebe</b>. Quien carga no
          aprueba: es el doble control de RN-39.
        </p>
      </header>

      <Panel titulo="El valor y su vigencia">
        <div className="tw:grid tw:gap-4 tw:sm:grid-cols-2">
          <Campo
            etiqueta="Clave"
            obligatorio
            mono
            ayuda="La que consulta el cálculo. Escribirla mal crea un parámetro que nadie lee."
          >
            <input value={clave} onChange={(e) => setClave(e.target.value)} />
          </Campo>

          <Campo etiqueta="Valor" obligatorio mono>
            <input value={valor} onChange={(e) => setValor(e.target.value)} />
          </Campo>

          <Campo
            etiqueta="Rige desde"
            obligatorio
            ayuda="La fecha del hecho, no la de captura: el cálculo usa la tabla vigente ese día."
          >
            <input type="date" value={desde} onChange={(e) => setDesde(e.target.value)} />
          </Campo>

          <Campo
            etiqueta="Rige hasta"
            ayuda="Vacío es sin fecha de cierre. No es lo mismo que hoy."
          >
            <input type="date" value={hasta} onChange={(e) => setHasta(e.target.value)} />
          </Campo>
        </div>

        {esRetroactivo && (
          <Nota tono="aviso" icono={<Info />}>
            <b>La vigencia arranca en el pasado.</b> Es válido —una tarifa publicada tarde rige
            desde antes— pero significa que el valor pasará a regir sobre <b>hechos ya
            registrados</b>. Si se equivocó de año, éste es el momento barato de corregirlo.
          </Nota>
        )}
      </Panel>

      <Panel titulo="El respaldo">
        <p className="tw:mb-4 tw:text-sm tw:text-tinta-mid">
          Sin esto la carga se rechaza. <b>Un parámetro sin documento que lo sostenga no se puede
          defender</b>: la cifra existe y nadie sabe de dónde salió.
        </p>

        <div className="tw:grid tw:gap-4 tw:sm:grid-cols-2">
          <Campo
            etiqueta="Fuente"
            obligatorio
            ayuda="Comunicado, acuerdo o tabla oficial. Con qué se cita el dato."
          >
            <input value={fuente} onChange={(e) => setFuente(e.target.value)} />
          </Campo>

          <Campo
            etiqueta="Verificada el"
            obligatorio
            ayuda="Cuándo alguien confirmó el dato contra la fuente. No es la fecha de hoy por omisión."
          >
            <input
              type="date"
              value={verificadoEl}
              onChange={(e) => setVerificadoEl(e.target.value)}
            />
          </Campo>

          <Campo
            etiqueta="Documento"
            obligatorio
            ayuda="El comunicado, el acuerdo o la tabla oficial. Quien apruebe tiene que poder abrirlo: si no está, la aprobación se bloquea."
          >
            <input
              type="file"
              onChange={(e) => setArchivo(e.target.files?.[0] ?? null)}
            />
          </Campo>
        </div>
      </Panel>

      <div>
        <Boton onClick={() => cargar.mutate()} cargando={cargar.isPending} disabled={!completo}>
          Cargar pendiente de aprobación
        </Boton>
      </div>

      {/* ── La línea de vigencias de la clave ─────────────────────────────────
          Va acá y no en otra pantalla porque **los tres rechazos se entienden mirándola**.
          «Solapa con el valor 15 % vigente desde el 01/01/2026» es un mensaje que se lee dos
          veces si no se tiene enfrente contra qué solapa. */}
      {clave.trim() !== '' && (
        <Panel titulo={`Lo que ya existe de «${clave.trim()}»`}>
          {isError ? (
            <Nota tono="riesgo" icono={<CircleAlert />}>
              No se pudo consultar la línea de vigencias.
            </Nota>
          ) : versiones === undefined ? (
            <p className="tw:text-sm tw:text-tinta-mid">Consultando…</p>
          ) : versiones.length === 0 ? (
            <p className="tw:text-sm tw:text-tinta-mid">
              Ninguna. <b>Sería la primera carga de esta clave</b>: hasta hoy el control que la
              usa está apagado.
            </p>
          ) : (
            <Tabla<Version>
              columnas={[
                {
                  id: 'valor',
                  cabecera: 'Valor',
                  celda: (v) => <span className="tw:font-mono">{v.valor}</span>,
                },
                {
                  id: 'vigencia',
                  cabecera: 'Vigencia',
                  celda: (v) =>
                    `${fecha(v.vigenteDesde)} — ${
                      v.vigenteHasta === null ? 'sin cierre' : fecha(v.vigenteHasta)
                    }`,
                },
                {
                  id: 'estado',
                  cabecera: 'Estado',
                  celda: (v) =>
                    v.estaAprobada ? (
                      <Pastilla tono="ok">en vigencia</Pastilla>
                    ) : (
                      // **Pendiente NO rige.** Si se leyera como «cargada» alguien creería
                      // que el valor ya se aplica, y el doble control quedaría de adorno.
                      <Pastilla tono="aviso">pendiente, no se aplica</Pastilla>
                    ),
                },
                {
                  id: 'fuente',
                  cabecera: 'Fuente',
                  celda: (v) => <span className="tw:text-tinta-mid">{v.respaldo.fuente}</span>,
                },
              ]}
              filas={versiones}
              claveDe={(v) => v.id}
            />
          )}
        </Panel>
      )}
    </div>
  );
}

/**
 * Sube el respaldo y devuelve el identificador con el que quedó guardado.
 *
 * El hash se calcula <b>acá</b> y el servidor lo vuelve a calcular sobre lo que recibió: si no
 * coinciden, rechaza. No es ceremonia — un PDF truncado en tránsito se ve como un PDF hasta que
 * alguien lo abre, y eso pasaría meses después, en una auditoría.
 */
async function subirRespaldo(archivo: File): Promise<string> {
  const bytes = await archivo.arrayBuffer();
  const resumen = await crypto.subtle.digest('SHA-256', bytes);
  const hash =
    'sha256:' +
    Array.from(new Uint8Array(resumen))
      .map((b) => b.toString(16).padStart(2, '0'))
      .join('');

  const cuerpo = new FormData();
  cuerpo.append('archivo', archivo);
  cuerpo.append('idAdjunto', ulid());
  // Va vacío a propósito: **un respaldo de parámetro no cuelga de ninguna transición**.
  cuerpo.append('idTransicion', '');
  cuerpo.append('hash', hash);
  cuerpo.append('clasificacion', 'ADMINISTRATIVO');
  cuerpo.append('capturadoEn', new Date().toISOString());

  const r = await pedir<{ id: string }>('/adjuntos', { method: 'POST', body: cuerpo });
  return r.id;
}

/** ULID del lado del cliente (`ADR-005`): el identificador nace donde nace el archivo. */
function ulid(): string {
  const CIFRAS = '0123456789ABCDEFGHJKMNPQRSTVWXYZ';
  let tiempo = Date.now();
  let salida = '';

  for (let i = 0; i < 10; i++) {
    salida = CIFRAS[tiempo % 32] + salida;
    tiempo = Math.floor(tiempo / 32);
  }

  const azar = crypto.getRandomValues(new Uint8Array(16));
  for (const b of azar) salida += CIFRAS[b % 32];

  return salida.slice(0, 26);
}

const hoy = (): string => new Date().toISOString().slice(0, 10);
const fecha = (d: string): string => new Date(`${d}T00:00:00`).toLocaleDateString('es-HN');

interface Version {
  id: string;
  clave: string;
  valor: string;
  vigenteDesde: string;
  vigenteHasta: string | null;
  cargadoPor: string;
  /** **Nulo es sin aprobar, y sin aprobar no rige.** */
  aprobadoPor: string | null;
  estaAprobada: boolean;
  respaldo: { fuente: string; verificadoEl: string };
}
