import type { ReactElement } from 'react';
import { useState } from 'react';
import { useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, Printer, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Nota, Panel, Vacio, avisar } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
import { usarQuienEjecuta } from '../../app/puesto';
import Qr from './Qr';

/**
 * `PT-023` — Emisión e impresión del <b>salvoconducto</b>.
 *
 * ── El primer documento físico del sistema ──────────────────────────────────
 * Premisa rectora 4: híbrido digital-papel <b>por diseño, no por parche</b>. Hasta acá todo
 * vivía en pantalla. Éste sale por una impresora, se dobla y se guarda en la guantera, y su
 * único destinatario —el agente del TSC o de la DNVT en un operativo— <b>no tiene usuario, no
 * se autentica y no verá nunca el expediente</b>.
 *
 * Por eso la pantalla es un papel, no un formulario: lo que se ve acá es exactamente lo que
 * sale impreso.
 *
 * ── Los dos mecanismos de verificación ──────────────────────────────────────
 * El QR resuelve al punto de verificación. Y <b>en zona sin señal no se puede escanear</b>:
 * `RN-25` obliga por eso a un código corto legible, que el agente anota y consulta al volver,
 * más datos suficientes para el control visual. La verificación en línea no puede ser el único
 * mecanismo en un país con la conectividad que documenta `NRM-09`.
 */
export default function Salvoconducto(): ReactElement {
  const { id = '' } = useParams();
  const quienEjecuta = usarQuienEjecuta();
  const cliente = useQueryClient();
  const [motivo, setMotivo] = useState('');

  const { data, isPending, isError } = useQuery({
    queryKey: ['salvoconducto', id],
    queryFn: () => pedir<Documento | SinEmitir>(`/misiones/${id}/salvoconducto`),
  });

  // ⚠️ El identificador entra por argumento y no se lee de una variable de más abajo: el
  // objeto de la mutación se construye en TODO render, incluido aquel en el que el documento
  // no existe y el componente sale temprano. Capturarlo por cierre dejaría una llamada que
  // revienta en el único caso en que nadie la mira.
  const reimprimir = useMutation({
    mutationFn: (salvoconducto: string) =>
      pedir(`/salvoconductos/${salvoconducto}/reimprimir`, {
        method: 'POST',
        body: JSON.stringify({
          ejecuta: quienEjecuta,
          motivo,
          momento: new Date().toISOString(),
        }),
      }),
    onSuccess: async () => {
      setMotivo('');
      await cliente.invalidateQueries({ queryKey: ['salvoconducto', id] });
      window.print();
    },
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo registrar la reimpresión.'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el salvoconducto.
      </Nota>
    );
  }

  if (isPending) return <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>;

  // El estrechamiento se hace ACA, una vez: de aca para abajo `data` es el documento.
  if (data.emitido === false) {
    return (
      <Vacio
        icono={<Printer />}
        titulo="Esta misión no tiene salvoconducto emitido"
        descripcion="El salvoconducto materializa un permiso firmado. Se emite desde el expediente, una vez que la máxima autoridad firmó."
      />
    );
  }

  const doc = data as Documento;

  const url = `${window.location.origin}/verificar/${doc.folio}`;

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      {/* ── Lo que NO se imprime ────────────────────────────────────────────
          Los controles viven fuera del papel. Un botón impreso en un documento oficial es
          ruido que un fiscalizador tiene que aprender a ignorar. */}
      <div className="tw:flex tw:flex-col tw:gap-4 print:tw:hidden">
        <header className="tw:flex tw:flex-col tw:gap-1">
          <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
            Salvoconducto {doc.folio}
          </h1>
          <p className="tw:text-sm tw:text-tinta-mid">
            Lo que ve acá es <b>exactamente lo que sale impreso</b>. Sin este papel en la mano
            no se despacha en día u hora inhábil: no hay excepción.
          </p>
        </header>

        {doc.folioProvisional && (
          <Nota tono="aviso" icono={<TriangleAlert />}>
            <b>El folio es provisional.</b> La delegación no tiene rango de folios de
            salvoconducto asignado, así que este número no es el correlativo oficial. El
            documento lo declara impreso — un folio inventado que se ve oficial es peor que uno
            que dice que no lo es.
          </Nota>
        )}

        {doc.estado !== 'Vigente' && (
          <Nota tono="riesgo" icono={<TriangleAlert />}>
            {doc.veredicto}
          </Nota>
        )}

        <div className="tw:flex tw:flex-wrap tw:items-end tw:gap-3">
          <Boton onClick={() => window.print()} icono={<Printer />}>
            Imprimir
          </Boton>

          <span className="tw:text-xs tw:text-tinta-mid">
            {doc.impresiones.length === 1
              ? 'Impreso 1 vez'
              : `Impreso ${doc.impresiones.length} veces`}
          </span>
        </div>

        <Panel titulo="Reimprimir">
          <div className="tw:flex tw:flex-col tw:gap-3">
            <p className="tw:text-sm tw:text-tinta-mid">
              La reimpresión <b>conserva el folio, el contenido y la huella</b> (RN-04): dos
              folios para un mismo permiso rompen la conciliación. Lo único que se agrega es
              quién, cuándo y por qué.
            </p>

            <Campo
              etiqueta="Por qué se reimprime"
              obligatorio
              ayuda="Una reimpresión sin motivo es indistinguible de una copia de más, y el conteo deja de significar algo."
            >
              <input value={motivo} onChange={(e) => setMotivo(e.target.value)} />
            </Campo>

            <div>
              <Boton
                onClick={() => reimprimir.mutate(doc.id)}
                cargando={reimprimir.isPending}
                disabled={motivo.trim() === ''}
                variante="secundario"
              >
                Registrar reimpresión e imprimir
              </Boton>
            </div>

            {doc.impresiones.length > 1 && (
              <ul className="tw:flex tw:flex-col tw:gap-1 tw:text-xs tw:text-tinta-mid">
                {doc.impresiones.map((i) => (
                  <li key={i.orden}>
                    <b>#{i.orden}</b> · {new Date(i.momento).toLocaleString('es-HN')} ·{' '}
                    {i.quien}
                    {/* Nulo **sólo** en la primera, que es la emisión misma. */}
                    {i.motivo === null ? ' · emisión' : ` · ${i.motivo}`}
                  </li>
                ))}
              </ul>
            )}
          </div>
        </Panel>
      </div>

      {/* ── El papel ────────────────────────────────────────────────────────
          Fondo blanco y tinta negra fijos, no tokens del tema: el papel es blanco siempre, y
          un documento que saliera con el tema oscuro gastaría el tóner y no se leería. */}
      <article className="tw:mx-auto tw:w-full tw:max-w-[190mm] tw:bg-white tw:p-8 tw:text-black print:tw:max-w-none print:tw:p-0">
        <header className="tw:flex tw:items-start tw:justify-between tw:gap-6 tw:border-b-2 tw:border-black tw:pb-4">
          <div>
            <p className="tw:text-xs tw:uppercase tw:tracking-widest">República de Honduras</p>
            <h2 className="tw:mt-1 tw:text-lg tw:font-bold tw:uppercase">
              Salvoconducto de circulación
            </h2>
            <p className="tw:text-xs">
              Permiso para circular en día u hora inhábil · Acuerdo de la máxima autoridad
            </p>
          </div>

          <div className="tw:text-right">
            <p className="tw:text-[10px] tw:uppercase tw:tracking-wide">Folio</p>
            <p className="tw:font-mono tw:text-base tw:font-bold">{doc.folio}</p>
            {doc.folioProvisional && (
              <p className="tw:mt-1 tw:border tw:border-black tw:px-1 tw:text-[9px] tw:font-bold tw:uppercase">
                Folio provisional
              </p>
            )}
          </div>
        </header>

        {/* ── La vigencia, arriba y grande ──────────────────────────────────
            Es <b>lo primero que compara un fiscalizador</b>: la fecha del papel contra la del
            control. Enterrarla entre los demás datos la vuelve inútil. */}
        <section className="tw:mt-5 tw:border-2 tw:border-black tw:p-3">
          <p className="tw:text-[10px] tw:uppercase tw:tracking-wide">Ampara la circulación</p>
          <p className="tw:text-base tw:font-bold">
            Desde el {fecha(doc.contenido.desde)} hasta el {fecha(doc.contenido.hasta)}
          </p>
          {doc.contenido.tramosInhabiles.length > 0 && (
            <p className="tw:mt-1 tw:text-xs">
              Tramos inhábiles cubiertos: <b>{doc.contenido.tramosInhabiles.join(' · ')}</b>
            </p>
          )}
        </section>

        <section className="tw:mt-5 tw:grid tw:gap-x-8 tw:gap-y-3 tw:text-sm tw:sm:grid-cols-2">
          <Renglon rotulo="Vehículo">{doc.contenido.vehiculo}</Renglon>
          <Renglon rotulo="Motorista">{doc.contenido.motorista}</Renglon>
          <Renglon rotulo="Destino">{doc.contenido.destino}</Renglon>
          <Renglon rotulo="Permiso">{doc.contenido.folioDelPermiso}</Renglon>
        </section>

        <section className="tw:mt-4 tw:text-sm">
          <p className="tw:text-[10px] tw:uppercase tw:tracking-wide">Motivo de la circulación</p>
          <p>{doc.contenido.justificacion}</p>
        </section>

        {/* ── La verificación ───────────────────────────────────────────────── */}
        <section className="tw:mt-6 tw:flex tw:items-start tw:gap-5 tw:border-t tw:border-black tw:pt-4">
          <Qr texto={url} tamano={110} />

          <div className="tw:flex tw:flex-col tw:gap-2 tw:text-xs">
            <div>
              <p className="tw:text-[10px] tw:uppercase tw:tracking-wide">
                Código de verificación
              </p>
              {/* Se dicta por teléfono cuando no hay señal para escanear. */}
              <p className="tw:font-mono tw:text-xl tw:font-bold tw:tracking-widest">
                {doc.codigoCorto}
              </p>
            </div>

            <p>
              Escanee el código, o consulte el código de verificación en{' '}
              <span className="tw:font-mono">{window.location.host}/verificar</span>.
              <b> Sin señal, anote el código y verifique después</b>: los datos impresos
              permiten el control visual.
            </p>

            <p className="tw:break-all tw:font-mono tw:text-[9px]">
              Huella del documento: {doc.huella}
            </p>
          </div>
        </section>

        {/* ── Firma y sello ─────────────────────────────────────────────────
            Espacio físico real, no una línea decorativa: acá va una firma de puño y letra y
            un sello húmedo, porque **no hay firma electrónica certificada en el país**
            (`NRM-08`) y la autorización es interna. */}
        <section className="tw:mt-10 tw:grid tw:gap-10 tw:text-center tw:text-xs tw:sm:grid-cols-2">
          <div>
            <div className="tw:mb-1 tw:h-16 tw:border-b tw:border-black" />
            <p className="tw:font-bold">{doc.contenido.firmadoPor}</p>
            <p>Máxima Autoridad</p>
            <p>Firmó el {fechaHora(doc.contenido.firmadoEn)}</p>
          </div>

          <div>
            <div className="tw:mb-1 tw:h-16 tw:border-b tw:border-black" />
            <p className="tw:font-bold">Sello de la institución</p>
            <p>Emitido por {doc.emitidoPor}</p>
            <p>el {fechaHora(doc.emitidoEn)}</p>
          </div>
        </section>

        <footer className="tw:mt-6 tw:border-t tw:border-black tw:pt-2 tw:text-[9px]">
          Este documento ampara <b>únicamente</b> al vehículo, al motorista, al destino y a la
          ventana consignados. Un relevo de motorista o una sustitución de vehículo lo dejan sin
          efecto y obligan a reemitirlo. Circular fuera de la ventana amparada es circular sin
          permiso, aunque el papel exista.
        </footer>
      </article>
    </div>
  );
}

function Renglon({
  rotulo,
  children,
}: {
  rotulo: string;
  children: React.ReactNode;
}): ReactElement {
  return (
    <div>
      <p className="tw:text-[10px] tw:uppercase tw:tracking-wide">{rotulo}</p>
      <p className="tw:font-medium">{children}</p>
    </div>
  );
}

const fecha = (d: string): string => new Date(`${d}T00:00:00`).toLocaleDateString('es-HN');
const fechaHora = (d: string): string => new Date(d).toLocaleString('es-HN');

/** **«No hay documento» es una respuesta, no una ausencia.** */
interface SinEmitir {
  emitido: false;
}

interface Documento {
  emitido?: true;
  id: string;
  folio: string;
  /** **Sin rango asignado.** El papel lo declara: uno inventado que se ve oficial es peor. */
  folioProvisional: boolean;
  huella: string;
  /** Ocho caracteres para dictar por teléfono cuando no hay señal. */
  codigoCorto: string;
  estado: 'Vigente' | 'Desactualizado' | 'Anulado';
  veredicto: string;
  contenido: {
    folioDelPermiso: string;
    vehiculo: string;
    motorista: string;
    destino: string;
    desde: string;
    hasta: string;
    tramosInhabiles: string[];
    justificacion: string;
    firmadoPor: string;
    firmadoEn: string;
  };
  emitidoPor: string;
  emitidoEn: string;
  impresiones: {
    orden: number;
    quien: string;
    momento: string;
    /** Nulo **sólo** en la primera, que es la emisión misma. */
    motivo: string | null;
  }[];
}
