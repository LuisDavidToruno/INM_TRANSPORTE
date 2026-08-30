import type { ReactElement } from 'react';
import { useMemo } from 'react';
import generador from 'qrcode-generator';

/**
 * Un QR, dibujado como SVG.
 *
 * ── Por qué SVG y no un `<img>` ─────────────────────────────────────────────
 * Porque esto se <b>imprime</b>. Un mapa de bits se rasteriza al tamaño de pantalla y sale
 * borroso del papel; un SVG lo dibuja la impresora a su propia resolución. Un QR borroso no lo
 * lee un teléfono de gama baja en una carretera al mediodía, que es la única condición en la
 * que este código tiene que funcionar.
 *
 * ── Y por qué se genera en el cliente ───────────────────────────────────────
 * <b>No hay red que consultar.</b> El despliegue es on-premise y el documento se emite desde
 * una delegación que puede estar sin conectividad (`RN-44`): un QR que dependiera de un
 * servicio externo no se imprimiría justamente donde más falta hace. `qrcode-generator` no
 * tiene dependencias y se empaqueta en el bundle.
 */
export default function Qr({
  texto,
  tamano = 120,
}: {
  readonly texto: string;
  /** Lado en píxeles CSS. En papel manda el `mm` del estilo de impresión. */
  readonly tamano?: number;
}): ReactElement {
  const { camino, modulos } = useMemo(() => {
    // Corrección de errores `M`: ~15 % del código puede estar dañado y seguir leyéndose. Es lo
    // que hace que un papel doblado en la guantera durante cinco días siga sirviendo.
    const codigo = generador(0, 'M');
    codigo.addData(texto);
    codigo.make();

    const n = codigo.getModuleCount();
    const partes: string[] = [];

    for (let f = 0; f < n; f++) {
      for (let c = 0; c < n; c++) {
        if (codigo.isDark(f, c)) partes.push(`M${c},${f}h1v1h-1z`);
      }
    }

    return { camino: partes.join(''), modulos: n };
  }, [texto]);

  return (
    <svg
      width={tamano}
      height={tamano}
      viewBox={`0 0 ${modulos} ${modulos}`}
      role="img"
      aria-label={`Código QR de verificación: ${texto}`}
      className="tw:shrink-0"
    >
      {/* Blanco explícito: el QR se lee por contraste, y sobre un fondo oscuro —el sistema
          tiene tema oscuro— dejaría de leerse. El papel es blanco siempre. */}
      <rect width={modulos} height={modulos} fill="#fff" />
      <path d={camino} fill="#000" />
    </svg>
  );
}
