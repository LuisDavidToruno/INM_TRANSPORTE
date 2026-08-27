/**
 * El barril de la biblioteca.
 *
 * Una pantalla importa de acá (`import { Boton, Panel } from '../ui'`) y no de
 * los archivos sueltos. No es cosmético: es lo que deja mover, renombrar o
 * partir un componente sin tocar las pantallas que lo usan.
 *
 * ⚠️ Lo que NO está en este barril es privado del sistema de diseño, aunque el
 * archivo lo exporte. `posicionFlotante` es el caso: lo usan los dos calendarios
 * y nada más. Sacarlo acá lo volvería API pública y habría que sostenerlo.
 */

/* ── Primitivos ─────────────────────────────────────────────────────────────
   Las piezas indivisibles. Si estás por escribir un `<button>` o un `<input>`
   a mano, lo que buscás está acá. */
export { default as Avatar, iniciales } from './Avatar';
export { default as Banda } from './Banda';
export { default as Boton, BotonIcono, EnlaceBoton, clasesBoton } from './Boton';
export { default as Campo } from './Campo';
export { default as Enlace } from './Enlace';
export { default as Icono } from './Icono';
export { default as Nota } from './Nota';
export { default as Panel } from './Panel';
export { default as Pastilla } from './Pastilla';
export { default as Rotulo } from './Rotulo';
export { default as TileIcono } from './TileIcono';
export { default as Vacio } from './Vacio';

/* ── Campos de formulario ─────────────────────────────────────────────────── */
export { default as CampoBusqueda } from './CampoBusqueda';
export { default as CampoFecha } from './CampoFecha';
export { default as RangoFechas } from './RangoFechas';
export { default as Segmentado } from './Segmentado';
export { default as SelectorBuscable } from './SelectorBuscable';

/* ── Compuestos ─────────────────────────────────────────────────────────────
   Resuelven un problema entero, no una pieza. */
export { default as CajonExpediente } from './CajonExpediente';
export { default as FilaKpis } from './FilaKpis';
export { default as LineaDeCarriles } from './LineaDeCarriles';
export { default as MenuAcciones } from './MenuAcciones';
export { default as Modal } from './Modal';
export { default as Paginador, ventanaDePaginas } from './Paginador';
export { default as PaletaComandos } from './PaletaComandos';
export { default as RastreadorEtapas } from './RastreadorEtapas';
export { default as Tabla } from './Tabla';
export { default as TarjetaOpcion } from './TarjetaOpcion';
export { TarjetaKpi, TarjetaConteo, EsqueletoTarjetasKpi } from './TarjetaKpi';

/* ── Estados de carga ───────────────────────────────────────────────────────
   Van juntos y con nombre propio porque la regla dice que el esqueleto tiene
   que medir LO MISMO que el contenido que reemplaza. Un `<div>` gris a mano no
   cumple eso y produce el salto de layout que estos existen para evitar. */
export {
  Esqueleto,
  EsqueletoFichas,
  EsqueletoKpis,
  EsqueletoLista,
  EsqueletoTabla,
} from './Esqueleto';

/* ── Shell de aplicación ────────────────────────────────────────────────────
   La estructura de la página: riel, barra y el marco que los contiene. */
export { default as BarraSuperior } from './BarraSuperior';
export { default as Riel } from './Riel';
export { default as SelectorApariencia } from './SelectorApariencia';
export { default as Shell } from './Shell';

/* ── Gráficos ───────────────────────────────────────────────────────────────
   `import()` dinámico cuando una gráfica se monta. Un proyecto que salga de
   LOKI y no dibuje nada no paga un byte.

   🚫 No importar `graficos/nucleo` de forma estática desde ningún lado: anula la
   separación, y no falla — el bundle crece en silencio y nadie se entera hasta
   que alguien mira el informe de compilación. */

/* ── Avisos ─────────────────────────────────────────────────────────────────
   `Avisos` se monta UNA vez en la raíz; `avisar` se llama desde donde sea. */
export { Avisos, avisar } from './avisos';

/* ── Vocabulario compartido ─────────────────────────────────────────────────
   Los catálogos de estado, etapa y tono. Un estado se decide SIEMPRE por su
   identificador y nunca por su texto: «Por aprobar» contiene «aprob», y una
   comparación de cadenas la pinta de aprobada. */
export {
  CLASE_TONO,
  ESTADO,
  ESTADO_CLIENTE,
  ESTADO_INTERNO,
  ETAPA,
  TOKEN_TONO,
} from './tipos';

export type {
  ColumnaDef,
  EstadoId,
  EtapaId,
  GrupoNav,
  IconoLucide,
  ItemNav,
  Miga,
  Operacion,
  Plazo,
  Tamano,
  Tono,
} from './tipos';

export type { AvatarProps } from './Avatar';
export type { BandaProps } from './Banda';
export type { BarraSuperiorProps } from './BarraSuperior';
export type { BotonIconoProps, BotonProps, EnlaceBotonProps } from './Boton';
export type { CajonExpedienteProps } from './CajonExpediente';
export type { CampoProps, PropsDelControl } from './Campo';
export type { CampoFechaProps } from './CampoFecha';
export type { EnlaceProps } from './Enlace';
export type { EsqueletoProps } from './Esqueleto';
export type { FilaKpisProps, KpiDato } from './FilaKpis';
export type {
  BarraDeCarril,
  CarrilDeLinea,
  LineaDeCarrilesProps,
} from './LineaDeCarriles';
export type { IconoProps } from './Icono';
export type { MenuAccionesProps } from './MenuAcciones';
export type { ModalProps } from './Modal';
export type { NotaProps } from './Nota';
export type { ItemPaleta, PaletaComandosProps } from './PaletaComandos';
export type { PanelProps } from './Panel';
export type { PastillaProps } from './Pastilla';
export type { RangoFechasProps } from './RangoFechas';
export type { EstadoEtapa, RastreadorEtapasProps } from './RastreadorEtapas';
export type { RielProps } from './Riel';
export type { RotuloProps } from './Rotulo';
export type { OpcionSegmentada, ValorSegmentado } from './Segmentado';
export type { OpcionBuscable } from './SelectorBuscable';
export type { ShellProps } from './Shell';
export type { TablaProps } from './Tabla';
export type { TarjetaKpiProps } from './TarjetaKpi';
export type { TarjetaOpcionProps } from './TarjetaOpcion';
export type { TileIconoProps } from './TileIcono';
export type { VacioProps } from './Vacio';
