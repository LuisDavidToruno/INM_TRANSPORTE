import type { Expediente } from '../dominio/mision';

/**
 * Datos de muestra para trabajar sin servidor.
 *
 * <b>Los nombres y las placas son inventados a propósito, y los casos no.</b> Cada
 * expediente de acá existe para mostrar una situación real que el diseño tiene que
 * resolver: el bloqueo de `BD-01`, la advertencia por espejo antiguo que **no**
 * bloquea, el solicitante con misiones sin liquidar, y el caso limpio.
 *
 * Un conjunto de muestra donde todo está bien no sirve para diseñar: esconde
 * justamente las pantallas que hay que ver antes de construirlas.
 */
/**
 * Las fechas se calculan contra hoy, no van fijas.
 *
 * Una bandeja de autorización con salidas del mes pasado se lee como sistema
 * abandonado, y el evaluador deja de creerle a la pantalla antes de mirar lo que
 * la pantalla resuelve. Un conjunto de muestra que envejece deja de servir para
 * enseñar a los tres días.
 */
const enDias = (dias: number, hora: number): string => {
  const fecha = new Date();
  fecha.setDate(fecha.getDate() + dias);
  fecha.setHours(hora, 0, 0, 0);
  return fecha.toISOString();
};

export function expedientesDeMuestra(): Expediente[] {
  return [
    {
      id: '01JQ8MISION0000000000000A',
      folio: 'CHO-2026-00087',
      estado: 'Solicitada',
      capturadaPor: 'Wendy Cárcamo',
      solicitanteDeDerecho: 'Rolando Discua',
      dependencia: 'Subgerencia de Operaciones',
      objetoDelTraslado: 'Traslado de 3 servidores y equipo de cómputo',
      destino: 'Choluteca',
      salidaPrevista: enDias(6, 7),
      retornoPrevisto: enDias(7, 18),
      diario: [
        transicion('T-01', 'Borrador', 'Wendy Cárcamo', enDias(-1, 8)),
        transicion('T-02', 'Solicitada', 'Wendy Cárcamo', enDias(-1, 9)),
      ],
      validaciones: [
        {
          clase: 'bloqueo',
          regla: 'BD-01',
          titulo: 'Usted es el solicitante de derecho',
          detalle:
            'La solicitud la capturó Wendy Cárcamo, pero está a su nombre. La segregación entre quien solicita y quien autoriza no admite excepción configurable: quien autorice tiene que ser el nivel inmediato superior.',
        },
        {
          clase: 'conforme',
          regla: 'BD-09',
          titulo: 'El tipo de vehículo puede mover lo solicitado',
          detalle: '3 pasajeros y carga liviana contra un pick-up doble cabina.',
        },
      ],
    },
    {
      id: '01JQ8MISION0000000000000B',
      folio: 'CHO-2026-00088',
      estado: 'Solicitada',
      capturadaPor: 'Marvin Zelaya',
      solicitanteDeDerecho: 'Marvin Zelaya',
      dependencia: 'Delegación de Gracias a Dios',
      objetoDelTraslado: 'Entrega de insumos a la posta de Puerto Lempira',
      destino: 'Puerto Lempira',
      salidaPrevista: enDias(4, 5),
      retornoPrevisto: enDias(8, 17),
      diario: [
        transicion('T-01', 'Borrador', 'Marvin Zelaya', enDias(-2, 16)),
        transicion('T-02', 'Solicitada', 'Marvin Zelaya', enDias(-2, 17)),
      ],
      validaciones: [
        {
          clase: 'advertencia',
          regla: 'RN-50',
          titulo: 'Su competencia se resolvió con una estructura de 98 horas de antigüedad',
          detalle:
            'El espejo de ARGOS no se sincroniza desde hace cuatro días. Un cambio de jefatura posterior no se ve acá. Si continúa, quedará registrado que usted autorizó con este dato — y esa constancia se imprime en la orden.',
        },
        {
          clase: 'advertencia',
          regla: 'RN-27',
          titulo: 'El solicitante tiene 2 misiones anteriores sin liquidar',
          detalle:
            'CHO-2026-00061 retornó hace 19 días y CHO-2026-00072 hace 6. No bloquea esta autorización; sí conviene saberlo antes de comprometer otro vehículo.',
        },
      ],
    },
    {
      id: '01JQ8MISION0000000000000C',
      folio: 'TEG-2026-00311',
      estado: 'Solicitada',
      capturadaPor: 'Wendy Cárcamo',
      solicitanteDeDerecho: 'Iris Maradiaga',
      dependencia: 'Gerencia Administrativa',
      objetoDelTraslado: 'Supervisión de flota en delegación regional',
      destino: 'Comayagua',
      salidaPrevista: enDias(5, 8),
      retornoPrevisto: enDias(5, 19),
      diario: [
        transicion('T-01', 'Borrador', 'Wendy Cárcamo', enDias(-1, 10)),
        transicion('T-02', 'Solicitada', 'Wendy Cárcamo', enDias(-1, 11)),
      ],
      validaciones: [
        {
          clase: 'conforme',
          regla: 'BD-01',
          titulo: 'Segregación entre solicitante y autorizador, conforme',
          detalle: 'Usted no capturó, no envió y no es el solicitante de derecho.',
        },
        {
          clase: 'conforme',
          regla: 'RN-50',
          titulo: 'Estructura de autorización de 2 horas de antigüedad',
          detalle: 'Dentro del umbral configurado de 24 horas.',
        },
      ],
    },
  ];
}

function transicion(id: string, destino: Expediente['estado'], ejecuta: string, momento: string) {
  // Ninguna de la muestra reserva: son transiciones de captura y autorizacion.
  return { id, destino, ejecuta, momento, motivo: null, vehiculoTomado: null, conductorTomado: null };
}
