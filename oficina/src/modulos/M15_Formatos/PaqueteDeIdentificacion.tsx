import type { ReactElement } from 'react';
import { useParams } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, Printer, TriangleAlert } from 'lucide-react';

import { Boton, Nota, Vacio } from '../../ui';
import { pedir } from '../../api/misiones';

/**
 * `RN-65` — el <b>paquete de identificación en carretera</b> del vehículo sin lámina.
 *
 * ── Por qué existe este papel ───────────────────────────────────────────────
 * Un vehículo del Estado sin lámina metálica que un agente detiene <b>no tiene cómo
 * identificarse</b>. La lámina es lo primero que se pide y lo único que normalmente hace falta;
 * sin ella, lo que queda es este paquete o la palabra del motorista.
 *
 * Y no es un caso raro: <b>hay desabastecimiento nacional</b>. La flota real circula así.
 *
 * ── Se arma, no se congela ──────────────────────────────────────────────────
 * A diferencia del salvoconducto —que congela lo que ampara porque materializa una firma— éste
 * no ampara nada: <b>describe</b>. Congelarlo produciría un papel que dice que la rotulación se
 * constató en marzo cuando en junio se volvió a constatar y faltaba la leyenda.
 */
export default function PaqueteDeIdentificacion(): ReactElement {
  const { id = '' } = useParams();

  const { data, isPending, isError } = useQuery({
    queryKey: ['paquete-de-identificacion', id],
    queryFn: () => pedir<Respuesta>(`/misiones/${id}/paquete-de-identificacion`),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo armar el paquete de identificación.
      </Nota>
    );
  }

  if (isPending) return <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>;

  if (!data.hayVehiculo) {
    return (
      <Vacio
        icono={<Printer />}
        titulo="La misión todavía no tiene vehículo reservado"
        descripcion="El paquete describe al vehículo que sale: hasta que la misión se programe no hay de cuál hablar."
      />
    );
  }

  const p = data;

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <div className="tw:flex tw:flex-col tw:gap-4 print:tw:hidden">
        <header className="tw:flex tw:flex-col tw:gap-1">
          <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
            Paquete de identificación en carretera
          </h1>
          <p className="tw:text-sm tw:text-tinta-mid">
            Lo que un agente puede comparar con el vehículo que tiene enfrente cuando{' '}
            <b>no hay lámina que pedir</b>.
          </p>
        </header>

        {/* Se dice para que el documento no se imprima por costumbre. */}
        {!p.haceFalta && (
          <Nota tono="ok">
            <b>Este vehículo lleva su lámina puesta.</b> No necesita el paquete: la lámina es su
            identificación, y un papel que nadie va a comparar con nada es papel de más.
          </Nota>
        )}

        {p.respaldo === null && p.haceFalta && (
          <Nota tono="riesgo" icono={<TriangleAlert />}>
            <b>Ningún respaldo cubre la ventana de esta misión.</b> El despacho está bloqueado
            por `RN-65` hasta que se registre uno vigente — y el documento lo dice impreso, en
            vez de mostrar el más reciente como si sirviera.
          </Nota>
        )}

        {p.identificacion !== null && p.identificacion.estado !== 'Constatada' && (
          <Nota tono="aviso" icono={<TriangleAlert />}>
            {p.identificacion.detalle}
          </Nota>
        )}

        <div>
          <Boton onClick={() => window.print()} icono={<Printer />}>
            Imprimir
          </Boton>
        </div>
      </div>

      {/* ── El papel ────────────────────────────────────────────────────────
          Blanco y negro fijos: el papel es blanco siempre. Mismo patrón que el
          salvoconducto — la hoja de impresión de `M-15` hace el resto. */}
      <article className="tw:mx-auto tw:w-full tw:max-w-[190mm] tw:bg-white tw:p-8 tw:text-black print:tw:max-w-none print:tw:p-0">
        <header className="tw:border-b-2 tw:border-black tw:pb-4">
          <p className="tw:text-xs tw:uppercase tw:tracking-widest">República de Honduras</p>
          <h2 className="tw:mt-1 tw:text-lg tw:font-bold tw:uppercase">
            Identificación de vehículo del Estado
          </h2>
          <p className="tw:text-xs">
            Vehículo sin lámina metálica · Documento de porte obligatorio en carretera
          </p>
        </header>

        {/* ── La identidad, arriba y grande ───────────────────────────────────
            Es lo que el agente compara con la calcomanía del vehículo. `RN-15`: la
            identidad del vehículo del Estado es el correlativo, no la placa — y acá eso
            deja de ser preferencia de diseño y pasa a ser lo único que hay. */}
        <section className="tw:mt-5 tw:border-2 tw:border-black tw:p-3">
          <p className="tw:text-[10px] tw:uppercase tw:tracking-wide">
            Correlativo institucional
          </p>
          <p className="tw:font-mono tw:text-2xl tw:font-bold">{p.correlativo}</p>
          <p className="tw:mt-1 tw:text-xs">
            Estado de la placa: <b>{p.estadoDePlacaTexto}</b>
            {p.placa !== null && (
              <>
                {' '}· Número asignado en el registro: <b>{p.placa}</b>
              </>
            )}
          </p>
        </section>

        <section className="tw:mt-5 tw:grid tw:gap-x-8 tw:gap-y-3 tw:text-sm tw:sm:grid-cols-2">
          <Renglon rotulo="Siglas">{p.siglas}</Renglon>
          <Renglon rotulo="Chasis / VIN">{p.chasis ?? 'no registrado'}</Renglon>
          <Renglon rotulo="Número de motor">{p.motor ?? 'no registrado'}</Renglon>
          <Renglon rotulo="Bien del inventario nacional">
            {p.bienDelInventario ?? 'no registrado'}
          </Renglon>
        </section>

        {/* ── El respaldo ───────────────────────────────────────────────────── */}
        <section className="tw:mt-5 tw:border tw:border-black tw:p-3">
          <p className="tw:text-[10px] tw:uppercase tw:tracking-wide">
            Documento que ampara la circulación sin lámina
          </p>

          {p.respaldo === null ? (
            <p className="tw:text-sm tw:font-bold">
              ⚠️ NINGUNO VIGENTE PARA ESTA VENTANA. Este vehículo no debió despacharse sin
              respaldo (RN-65).
            </p>
          ) : (
            <>
              <p className="tw:text-sm tw:font-bold">
                {p.respaldo.tipo} {p.respaldo.folio}
              </p>
              <p className="tw:text-xs">
                Emitido por {p.respaldo.emisor} · Vigente del{' '}
                {fecha(p.respaldo.vigenteDesde)} al{' '}
                {p.respaldo.vigenteHasta === null
                  ? 'sin fecha declarada'
                  : fecha(p.respaldo.vigenteHasta)}
              </p>
            </>
          )}
        </section>

        <section className="tw:mt-5 tw:grid tw:gap-x-8 tw:gap-y-3 tw:text-sm tw:sm:grid-cols-2">
          <Renglon rotulo="Dependencia">{p.dependencia}</Renglon>
          <Renglon rotulo="Motorista">{p.motorista ?? 'sin asignar'}</Renglon>
          <Renglon rotulo="Destino">{p.destino}</Renglon>
          <Renglon rotulo="Ventana de la misión">
            {fecha(p.desde)} al {fecha(p.hasta)}
          </Renglon>
        </section>

        {/* ── La rotulación ───────────────────────────────────────────────────
            `RN-18`. Va impreso porque es lo que el agente mira: las franjas, la leyenda,
            las siglas y el correlativo son lo que distingue a la vista un vehículo del
            Estado de uno particular. */}
        <section className="tw:mt-5 tw:text-sm">
          <p className="tw:text-[10px] tw:uppercase tw:tracking-wide">
            Identificación institucional constatada
          </p>

          {p.identificacion === null ? (
            <p>No se pudo evaluar.</p>
          ) : (
            <>
              <p>{p.identificacion.detalle}</p>

              {p.identificacion.faltantes.length > 0 && (
                <p className="tw:mt-1 tw:font-bold">
                  ⚠️ Falta: {p.identificacion.faltantes.join(', ')}
                </p>
              )}
            </>
          )}
        </section>

        <footer className="tw:mt-8 tw:border-t tw:border-black tw:pt-2 tw:text-[9px]">
          Este documento <b>no autoriza la circulación</b>: identifica al vehículo como bien del
          Estado cuando no porta lámina metálica. La autorización del viaje consta en la Orden de
          Misión, y la circulación en día u hora inhábil, en el salvoconducto.
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

/** **«No hay vehículo» es una respuesta, no una ausencia.** */
interface SinVehiculo {
  hayVehiculo: false;
}

interface Paquete {
  hayVehiculo: true;
  /** Si el vehículo **necesita** el paquete. Con lámina puesta, no. */
  haceFalta: boolean;
  correlativo: string;
  siglas: string;
  chasis: string | null;
  motor: string | null;
  bienDelInventario: string | null;
  /** **Nula no es «sin lámina»**: son los dos datos que `RN-64` separa. */
  placa: string | null;
  estadoDePlaca: string;
  estadoDePlacaTexto: string;
  /** **Nulo es que ninguno cubre la ventana**, y el papel lo dice. */
  respaldo: {
    tipo: string;
    emisor: string;
    folio: string;
    vigenteDesde: string;
    /** **Nulo no es «para siempre»**: es un provisional sin fecha declarada. */
    vigenteHasta: string | null;
    adjunto: string | null;
  } | null;
  dependencia: string;
  motorista: string | null;
  desde: string;
  hasta: string;
  destino: string;
  /** **Nulo es que no se pudo evaluar**, distinto de «está bien». */
  identificacion: {
    estado: string;
    faltantes: string[];
    sinConstatar: string[];
    caducaEl: string | null;
    detalle: string;
  } | null;
}

type Respuesta = Paquete | SinVehiculo;
