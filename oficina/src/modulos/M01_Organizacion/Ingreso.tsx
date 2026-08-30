import type { ReactElement } from 'react';
import { useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router';
import { CircleAlert, IdCard, TriangleAlert, UserX } from 'lucide-react';

import { Campo, Nota, Panel, SelectorBuscable, TarjetaOpcion, Vacio } from '../../ui';
import { pedir } from '../../api/misiones';
import { usarPuesto } from '../../app/puesto';

/**
 * `PT-001` — Ingreso y selección de puesto vigente.
 *
 * ── `R-1`, que es toda la pantalla ──────────────────────────────────────────
 * <i>«No hay un menú único. Hay una raíz por puesto.»</i> Los permisos se otorgan al puesto y
 * una persona puede ocupar varios: el Jefe de Transporte que además es custodio <b>ve dos
 * raíces distintas, no una mezclada</b>. Mezclarlas produciría un menú que ninguno de los dos
 * puestos tiene, y un alcance de datos que es la unión de dos permisos que nadie otorgó junto.
 *
 * ── Y lo que esta pantalla no es ────────────────────────────────────────────
 * <b>No es un inicio de sesión.</b> No hay contraseña, no hay verificación, y elegir a otra
 * persona no requiere nada. Se dice en la pantalla en vez de disimularse con una caja de
 * contraseña que no valida: una pantalla que <i>parece</i> autenticar es peor que una que
 * declara no hacerlo, porque hace creer que hay un control donde no lo hay.
 */
export default function Ingreso(): ReactElement {
  const { elegido, elegir } = usarPuesto();
  const navegar = useNavigate();

  const [persona, setPersona] = useState(() => elegido?.persona ?? '');

  const gente = useQuery({
    queryKey: ['puesto', 'personas'],
    queryFn: () => pedir<Persona[]>('/puesto/personas'),
  });

  const suyos = useQuery({
    queryKey: ['puesto', 'de', persona],
    queryFn: () => pedir<PuestosDeLaPersona>(`/puesto/de/${persona}`),
    enabled: persona !== '',
  });

  // ── El puesto único no se hace elegir ─────────────────────────────────────
  // El dictamen pide «aviso de puesto único». Obligar a confirmar una lista de una sola opción
  // es un clic que no decide nada, todos los días, para la mayoría de la gente.
  const solo = suyos.data?.puestos.length === 1 ? suyos.data.puestos[0] : undefined;
  const unico = solo?.enElEspejo === true ? solo : null;

  useEffect(() => {
    if (unico === null || persona === '') return;

    elegir({ persona, puesto: unico.puesto, denominacion: unico.denominacion });
    navegar('/inicio', { replace: true });
  }, [unico, persona, elegir, navegar]);

  return (
    <div className="tw:mx-auto tw:flex tw:max-w-3xl tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          ¿Con qué puesto va a trabajar?
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Los permisos se otorgan al puesto, no a la persona. De esta elección dependen{' '}
          <b>el menú, el alcance de datos y quién queda registrado</b> como autor de cada acto.
        </p>
      </header>

      <Nota tono="aviso" icono={<TriangleAlert />}>
        <b>Esto no es un inicio de sesión.</b> Todavía no hay autenticación: nadie verifica que
        quien elige un puesto tenga derecho a ocuparlo, y elegir a otra persona no pide nada.
        Mientras siga así, el alcance de datos <b>filtra, pero no protege</b>.
      </Nota>

      <Panel titulo="Quién entra">
        <div className="tw:sm:max-w-md">
          <Campo
            etiqueta="Persona"
            ayuda="Sale del organigrama: son quienes ocupan algún puesto hoy."
          >
            {(control) => (
              <SelectorBuscable
                {...control}
                valor={persona}
                onCambio={setPersona}
                opciones={(gente.data ?? []).map((p) => ({
                  valor: p.persona,
                  etiqueta:
                    p.puestos.length === 1
                      ? `${p.persona} — ${p.puestos[0]}`
                      : `${p.persona} — ${p.puestos.length} puestos`,
                  buscarTambien: p.puestos.join(' '),
                }))}
                vacio="Elija a quién representa…"
              />
            )}
          </Campo>
        </div>
      </Panel>

      {persona === '' ? null : suyos.isError ? (
        <Nota tono="riesgo" icono={<CircleAlert />}>
          No se pudieron cargar los puestos de esa persona.
        </Nota>
      ) : suyos.isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Buscando sus puestos…</p>
      ) : !suyos.data.conocida ? (
        <Vacio
          icono={<UserX />}
          titulo="El organigrama no conoce a esa persona"
          descripcion="Nunca ocupó ningún puesto. No es lo mismo que no tener puesto hoy: revise el identificador."
        />
      ) : suyos.data.puestos.length === 0 ? (
        <Vacio
          icono={<UserX />}
          titulo="No ocupa ningún puesto hoy"
          descripcion="La persona existe en el organigrama y no tiene ninguna asignación vigente. Un usuario sin puesto vigente es un usuario sin permisos — no se borra, porque sus actos históricos lo referencian."
        />
      ) : (
        <>
          {suyos.data.puestos.length > 1 && (
            <p className="tw:text-sm tw:text-tinta-mid">
              Ocupa <b>{suyos.data.puestos.length} puestos</b>. Cada uno tiene su propia raíz y
              su propio alcance de datos — no se mezclan.
            </p>
          )}

          <div className="tw:grid tw:gap-3 tw:sm:grid-cols-2">
            {suyos.data.puestos.map((p) => (
              <TarjetaOpcion
                key={p.puesto}
                icono={<IdCard />}
                titulo={p.denominacion ?? p.puesto}
                subtitulo={p.competencias
                  .map((c) => `${c.rol} · ${c.alcance}`)
                  .join(' — ')}
                descripcion={descripcionDe(p)}
                pie={avisoDe(p)}
                tono={p.enElEspejo ? 'neutro' : 'riesgo'}
                etiquetaAccion={p.enElEspejo ? 'Entrar con este puesto' : 'No se puede entrar'}
                // `ocupado` es lo que el componente usa para inhabilitar el botón. Un puesto
                // fuera del espejo no puede resolver su alcance, y entrar con él mostraría
                // una lista vacía sin que nadie sepa por qué.
                ocupado={!p.enElEspejo}
                textoOcupado="Sin datos en el espejo"
                onElegir={() => {
                  elegir({
                    persona,
                    puesto: p.puesto,
                    denominacion: p.denominacion,
                  });
                  navegar('/inicio');
                }}
              />
            ))}
          </div>
        </>
      )}

      {elegido !== null && (
        <p className="tw:flex tw:items-center tw:gap-1.5 tw:text-xs tw:text-tinta-mid">
          <IdCard className="tw:size-4" aria-hidden />
          Ahora mismo trabaja como <b>{elegido.denominacion ?? elegido.puesto}</b>, en nombre de{' '}
          {elegido.persona}.
        </p>
      )}
    </div>
  );
}

function descripcionDe(p: PuestoVigente): string {
  if (!p.enElEspejo)
    return (
      'No está en el espejo del organigrama, así que no se sabe a qué unidad pertenece y su ' +
      'alcance de datos no se puede resolver.'
    );

  const donde = p.delegacion === null ? p.unidad : `${p.unidad} · delegación ${p.delegacion}`;
  const raices = p.raices.map((r) => r.nombre).join(' y ');

  return raices === '' ? (donde ?? '') : `${donde}. Entra a ${raices}.`;
}

/**
 * Lo que hay que advertir de este puesto, si hay algo.
 *
 * Un rol sin raíz declarada deja a su ocupante sin punto de entrada, y eso es una brecha del
 * mapa de navegación — no una pantalla que se olvidó de cargar. Callarlo haría que el puesto
 * entrara a una aplicación sin lugar propio y pareciera un defecto de programación.
 */
function avisoDe(p: PuestoVigente): string {
  if (!p.enElEspejo) return 'Falta en el espejo';

  if (p.rolesSinRaiz.length > 0)
    return `El mapa no declara raíz para ${p.rolesSinRaiz.join(', ')}`;

  return p.raices.map((r) => r.pantalla).filter(Boolean).join(' · ');
}

interface Persona {
  persona: string;
  puestos: string[];
}

interface PuestoVigente {
  puesto: string;
  /** Nulos cuando el puesto no está en el espejo. No se sustituyen por el identificador. */
  denominacion: string | null;
  unidad: string | null;
  delegacion: string | null;
  enElEspejo: boolean;
  competencias: { rol: string; alcance: string }[];
  raices: { pantalla: string | null; nombre: string; porQue: string }[];
  /** Roles que el mapa de navegación no cubre. */
  rolesSinRaiz: string[];
}

interface PuestosDeLaPersona {
  persona: string;
  fecha: string;
  /** **Falso es «nunca tuvo ninguno»**, no «no tiene hoy». */
  conocida: boolean;
  puestos: PuestoVigente[];
}
