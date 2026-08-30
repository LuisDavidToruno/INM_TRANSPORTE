import { Suspense, lazy } from 'react';
import type { ReactElement } from 'react';
import { Navigate, Route, Routes, useLocation } from "react-router";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useQuery } from '@tanstack/react-query';
import { CalendarClock, CalendarX2, ClipboardCheck, FileCheck2, Palette, LayoutDashboard, Truck, Fuel, Milestone, FileSearch, Archive, TriangleAlert, HandCoins, ScrollText, LayoutList, IdCard, Scale, Users, ShieldAlert, Inbox, Route as RutaIcono, SlidersHorizontal, Radio, Home, Search, ShieldX, FileStack, Hash, GitCompareArrows, HardDrive, PlugZap, ShieldQuestion, Eye, ScrollText as Lista, Globe, HeartPulse, FilePlus2, ShieldCheck, Stamp, ScanLine } from 'lucide-react';

import { Avisos, Nota, Shell, avisar } from '../ui';
import type { GrupoNav, Miga } from '../ui';
import logo from '../marca/argos/logo.png';

// La vitrina es documentación del sistema de diseño, no la aplicación. Se carga
// aparte para que quien entra a autorizar un expediente no descargue la galería.
const Vitrina = lazy(() => import('../vitrina/Vitrina'));
import Bandeja from '../modulos/M06_Autorizacion/Bandeja';
import Expediente from '../modulos/M06_Autorizacion/Expediente';
import Asignacion from '../modulos/M07_Programacion/Asignacion';
import Cola from '../modulos/M07_Programacion/Cola';
import Tablero from '../modulos/M07_Programacion/Tablero';
import Padron from '../modulos/M03_Flota/Padron';
import PrestamosPantalla from '../modulos/M03_Flota/Prestamos';
import Titulos from '../modulos/M03_Flota/Titulos';
import Puestos from '../modulos/M01_Organizacion/Puestos';
import FirmaDePermisos from '../modulos/M07_Programacion/FirmaDePermisos';
import Salvoconducto from '../modulos/M15_Formatos/Salvoconducto';
import Verificacion from '../modulos/M15_Formatos/Verificacion';
import BandejaDeTareas from '../modulos/M01_Organizacion/Bandeja';
import IntentosBloqueados from '../modulos/M14_Auditoria/IntentosBloqueados';
import Ingreso from '../modulos/M01_Organizacion/Ingreso';
import InicioDelPuesto from '../modulos/M01_Organizacion/InicioDelPuesto';
import Buscador from '../modulos/M01_Organizacion/Buscador';
import Bloqueos from '../modulos/M01_Organizacion/Bloqueos';
import MisSolicitudes from '../modulos/M06_Solicitudes/MisSolicitudes';
import Folios from '../modulos/M06_Solicitudes/Folios';
import Conflictos from '../modulos/M16_Sincronizacion/Conflictos';
import Dispositivos from '../modulos/M16_Sincronizacion/Dispositivos';
import Espejos from '../modulos/M16_Sincronizacion/Espejos';
import CamposDelManifiesto from '../modulos/M17_PersonasExternas/CamposDelManifiesto';
import Accesos from '../modulos/M17_PersonasExternas/Accesos';
import Manifiesto from '../modulos/M17_PersonasExternas/Manifiesto';
import HabeasData from '../modulos/M17_PersonasExternas/HabeasData';
import Transparencia from '../modulos/M17_PersonasExternas/Transparencia';
import Salud from '../modulos/M02_Parametros/Salud';
import CargarParametro from '../modulos/M02_Parametros/CargarParametro';
import Aprobaciones from '../modulos/M02_Parametros/Aprobaciones';
import { ProveedorDelPuesto, usarPuesto } from './puesto';
import TableroDeSeguimiento from '../modulos/M19_Seguimiento/Tablero';
import EnRuta from '../modulos/M19_Seguimiento/EnRuta';
import Pista from '../modulos/M14_Auditoria/Pista';
import RastroDelExpediente from '../modulos/M14_Auditoria/RastroDelExpediente';
import ParametrosNormativos from '../modulos/M14_Auditoria/ParametrosNormativos';
import ExpedienteDeVehiculo from '../modulos/M03_Flota/Expediente';
import PadronDeMotoristas from '../modulos/M05_Motoristas/Padron';
import MatrizDeLicencias from '../modulos/M05_Motoristas/Matriz';
import Mapa from '../pantallas/Mapa';
import EnDesarrollo from '../pantallas/EnDesarrollo';
import Fondos from '../modulos/M09_Combustible/Fondos';
import Peajes from '../modulos/M18_Peajes/Peajes';
import Conciliacion from '../modulos/M14_Auditoria/Conciliacion';
import SaldoDeAperturaPantalla from '../modulos/M14_Auditoria/SaldoDeApertura';
import CierreDeEjercicioPantalla from '../modulos/M14_Auditoria/CierreDeEjercicio';
import IncidentesPantalla from '../modulos/M12_Incidentes/Incidentes';
import ColaDeCierre from '../modulos/M13_Cierre/Cola';
import Cierre from '../modulos/M13_Cierre/Cierre';
import { bandejaDeAutorizacion, origenDeDatos, pedir } from '../api/misiones';

/**
 * El shell de SIGTI en la oficina.
 *
 * ── Quién es el usuario todavía no viene del servidor ────────────────────────
 * `M-01` no está construido, así que la jefatura está fija acá. En cuanto exista
 * autenticación, esto sale del token y el menú se arma con las **capacidades**
 * que publique el servidor, nunca con el rol (`ADR-008`): un botón ofrecido que
 * el servidor rechaza se lee como falla del sistema, no como regla.
 */

const cliente = new QueryClient({
  defaultOptions: {
    queries: {
      // El expediente es un acto administrativo, no un panel de métricas: no se
      // refresca solo bajo el cursor de quien está por decidir sobre él.
      refetchOnWindowFocus: false,
      staleTime: 30_000,
      retry: 1,
    },
  },
});

// ⚠️ **El usuario ya no está cableado.** Sale del puesto elegido en `PT-001`, que es lo que
// `R-1` exige: los permisos se otorgan al puesto y de esa elección depende toda la raíz.
//
// Antes de esto había una constante acá y nueve pantallas pasando el mismo nombre a mano como
// autor de cada acto — lo que dejaba **inerte la segregación de funciones**: `I-01` a `I-19`
// comparan al actor contra los actos previos del expediente, y si el actor es siempre la misma
// constante, lo que comparan no es a nadie.
function usuarioDelPuesto(elegido: ReturnType<typeof usarPuesto>['elegido']) {
  return elegido === null
    ? { nombre: 'Sin puesto', rol: 'Elija con qué puesto trabaja' }
    : { nombre: elegido.persona, rol: elegido.denominacion ?? elegido.puesto };
}

const MARCA = { logo, nombre: 'SIGTI', bajada: 'Transporte institucional' };

export default function App(): ReactElement {
  return (
    <QueryClientProvider client={cliente}>
      <ProveedorDelPuesto>
        <Interior />
      </ProveedorDelPuesto>
      <Avisos />
    </QueryClientProvider>
  );
}

function Interior(): ReactElement {
  const { pathname } = useLocation();
  const { elegido } = usarPuesto();

  // El contador del riel sale del mismo dato que la bandeja, no de un conteo
  // aparte: dos fuentes para el mismo número divergen el día que una falla.
  const { data } = useQuery({
    queryKey: ['bandeja-autorizacion'],
    queryFn: bandejaDeAutorizacion,
  });

  // Mismo criterio que el contador de autorización: sale del mismo dato que la pantalla, no de
  // un conteo aparte. Dos fuentes para el mismo número divergen el día que una falla.
  const tareas = useQuery({
    queryKey: ['tareas'],
    queryFn: () => pedir<{ pendientes: number }>('/tareas'),
  });

  const grupos: GrupoNav[] = [
    {
      // Sin título: son las transversales de `R-1` y `R-2`, no un módulo. El inicio va
      // primero porque es a donde entra cada puesto a ver qué le toca.
      titulo: 'Mi puesto',
      items: [
        { texto: 'Inicio', icono: <Home />, href: '/inicio' },
        { texto: 'Buscar expedientes', icono: <Search />, href: '/buscar' },
        { texto: 'Patrón de bloqueo', icono: <ShieldX />, href: '/bloqueos' },
      ],
    },
    {
      // `M-01` va primero porque es la base: sin puesto no hay permiso, y sin competencia no
      // hay quién ejecute ningún acto de los demás módulos.
      titulo: 'M-01 Organización y seguridad',
      items: [
        // Va primero: es lo que hay que ATENDER. Las competencias se configuran una vez y
        // se miran de vez en cuando; una tarea escalada espera a alguien hoy.
        {
          texto: 'Tareas pendientes',
          icono: <Inbox />,
          href: '/tareas',
          contador: tareas.data?.pendientes,
          accionable: true,
        },
        { texto: 'Puestos y competencias', icono: <Users />, href: '/organizacion' },
      ],
    },
    {
      titulo: 'M-06 Solicitud y autorización',
      items: [
        {
          texto: 'Mis solicitudes',
          icono: <FileStack />,
          href: '/solicitudes',
        },
        {
          texto: 'Control de folios',
          icono: <Hash />,
          href: '/folios',
        },
        {
          texto: 'Bandeja de autorización',
          icono: <ClipboardCheck />,
          href: '/autorizacion',
          contador: data?.length,
          accionable: true,
        },

        // Va pegado a la bandeja de autorizacion porque es la otra firma del mismo flujo,
        // y quien firma permisos entra al sistema para esto y para nada mas.
        {
          texto: 'Firma de permisos',
          icono: <Stamp />,
          href: '/permisos/firmar',
          accionable: true,
        },

        // El punto de verificación vive en el riel porque **también se usa desde adentro**:
        // quien recibe una llamada de un agente en carretera teclea acá el código que le
        // dictan. El QR entra por la misma ruta sin pasar por el riel.
        {
          texto: 'Verificar salvoconducto',
          icono: <ScanLine />,
          href: '/verificar',
        },
      ],
    },
    {
      // `M-03` va antes que `M-07`: la flota es el sujeto del sistema --«SIGTI cuida de
      // todo lo referente a los vehiculos»-- y las misiones son lo que se hace con ella.
      titulo: 'M-03 Flota vehicular',
      items: [
        { texto: 'Padrón de flota', icono: <Truck />, href: '/flota' },
        // Los títulos van pegados al padrón: son la respuesta a «¿es nuestro?», y de ella
        // cuelga cuál de los dos terminales ofrece el propio padrón al dar de baja.
        { texto: 'Títulos de tenencia', icono: <ScrollText />, href: '/titulos' },
        { texto: 'Préstamos', icono: <HandCoins />, href: '/prestamos' },
      ],
    },
    {
      // `M-05` va pegado a `M-03`: son los dos expedientes que la asignación cruza, y quien
      // programa mira el uno para decidir sobre el otro.
      titulo: 'M-05 Motoristas',
      items: [
        { texto: 'Padrón de motoristas', icono: <IdCard />, href: '/motoristas' },
        { texto: 'Matriz de licencias', icono: <Scale />, href: '/motoristas/matriz' },
      ],
    },
    {
      // `M-09` va después de la flota y antes de la programación: el fondo es la
      // precondición de todo lo demás -- sin fondo aprobado vigente no se emite un vale, y
      // sin vale no se despacha lo que necesita combustible.
      titulo: 'M-09 Combustible',
      items: [
        { texto: 'Fondo del período', icono: <Fuel />, href: '/combustible' },
      ],
    },
    {
      // `M-18` va pegado a `M-09` porque son el mismo bolsillo visto en dos monedas: el
      // fondo cubre galones y el peaje se paga en efectivo de la misma misión. Quien mira
      // uno tiene que poder mirar el otro sin cambiar de sección.
      titulo: 'M-18 Peajes',
      items: [
        { texto: 'Catálogo y tarifas', icono: <Milestone />, href: '/peajes' },
      ],
    },
    {
      titulo: 'M-07 Programación y despacho',
      items: [
        // El tablero va PRIMERO: es la raíz de `ACT-05` en el mapa de navegación, y la
        // cola de programación es de `ACT-04`. Son dos personas distintas entrando al
        // mismo módulo, y el orden dice cuál abre cada una al empezar el día.
        { texto: 'Tablero de despacho', icono: <LayoutDashboard />, href: '/despacho' },
        { texto: 'Cola de programación', icono: <CalendarClock />, href: '/programacion' },
      ],
    },
    {
      titulo: 'M-16 Sincronización',
      items: [
        { texto: 'Registros que no coinciden', icono: <GitCompareArrows />, href: '/conflictos' },
        { texto: 'Envíos de los equipos', icono: <HardDrive />, href: '/dispositivos' },
        { texto: 'Datos de otros sistemas', icono: <PlugZap />, href: '/espejos' },
      ],
    },
    {
      titulo: 'M-17 Personas externas',
      items: [
        { texto: 'Quiénes van', icono: <Lista />, href: '/manifiesto' },
        { texto: 'Qué se pregunta', icono: <ShieldQuestion />, href: '/manifiesto/campos' },
        { texto: 'Quién vio qué', icono: <Eye />, href: '/manifiesto/accesos' },
        { texto: 'Hábeas data', icono: <Scale />, href: '/habeas-data' },
        { texto: 'Transparencia y depuración', icono: <Globe />, href: '/transparencia' },
      ],
    },
    {
      titulo: 'M-19 Seguimiento en ruta',
      items: [
        { texto: 'Tablero en ruta', icono: <Radio />, href: '/seguimiento' },
      ],
    },
    {
      // M-12 va antes del cierre: el incidente abierto es lo que impide cerrar limpio, y quien
      // liquida necesita verlo primero. `RN-70` y `RN-75` lo vuelven precondición del cierre,
      // no contexto: una interrupción sin desenlace impide producir el saldo de apertura.
      titulo: 'M-12 Incidentes',
      items: [
        { texto: 'Expedientes', icono: <TriangleAlert />, href: '/incidentes' },
      ],
    },
    {
      titulo: 'M-13 Liquidación y cierre',
      items: [
        { texto: 'Cola de cierre', icono: <FileCheck2 />, href: '/cierre' },
      ],
    },
    {
      // `M-14` va al final, después del cierre: es lo que se mira **después** de que los
      // expedientes cerraron. Conciliar contra el proveedor es lo que revela que un
      // expediente completo y coherente pudo ser falso.
      titulo: 'M-14 Auditoría',
      items: [
        // Va primero del grupo: es lo que Auditoría Interna abre para ver si el control
        // opera, y una pista de intentos escondida al final es una pista que no se mira.
        // El orden es el de la ficha de `ACT-12`: primero la cadena de un expediente, que es
        // lo que revisa, y después lo transversal.
        { texto: 'Rastro del expediente', icono: <RutaIcono />, href: '/rastro' },
        { texto: 'Pista de auditoría', icono: <FileSearch />, href: '/pista' },
        { texto: 'Intentos bloqueados', icono: <ShieldAlert />, href: '/intentos-bloqueados' },
        { texto: 'Parámetros normativos', icono: <SlidersHorizontal />, href: '/parametros-normativos' },
        { texto: 'Cargar parámetro', icono: <FilePlus2 />, href: '/parametros/cargar' },
        { texto: 'Poner en vigencia', icono: <ShieldCheck />, href: '/parametros/aprobar' },
        { texto: 'Qué está mal', icono: <HeartPulse />, href: '/salud' },
        { texto: 'Fuentes externas', icono: <FileSearch />, href: '/conciliacion' },
        { texto: 'Saldo de apertura', icono: <Archive />, href: '/saldo-de-apertura' },
        // El cierre de ejercicio va al final del grupo: es lo que se produce DESPUES de que
        // el saldo esta, porque el acta lo cita. Ofrecerlo antes invita a producir un acta
        // que no tiene contra que cuadrar.
        { texto: 'Cierre de ejercicio', icono: <CalendarX2 />, href: '/cierre-de-ejercicio' },
      ],
    },
    {
      // El mapa va con la vitrina y no dentro de un módulo: no es una pantalla de operar sino
      // el estado de construcción del sistema entero, y meterlo bajo M-xx daría a entender que
      // pertenece a ese módulo.
      items: [
        { texto: 'Mapa de pantallas', icono: <LayoutList />, href: '/pantallas' },
        { texto: 'Sistema de diseño', icono: <Palette />, href: '/sistema-diseno' },
      ],
    },
  ];

  return (
    <Shell
      marca={MARCA}
      usuario={usuarioDelPuesto(elegido)}
      grupos={grupos}
      activo={pathname}
      migas={migasDe(pathname)}
      // La paleta de comandos la monta la aplicación, no el shell. Todavía no
      // existe: sin `M-01` no hay contra qué buscar, y una paleta que devuelve
      // siempre vacío enseña a no usarla.
      onBuscar={() => avisar.info('La búsqueda global llega con M-01.')}
    >
      {origenDeDatos === 'muestra' && (
        <div className="tw:mb-5">
          <Nota tono="info">
            Datos de muestra. La API todavía no está conectada — defina{' '}
            <code className="tw:font-mono tw:text-xs">VITE_API</code> para apuntar al servidor.
          </Nota>
        </div>
      )}

      <Routes>
        <Route path="/" element={<Navigate to="/autorizacion" replace />} />
        <Route path="/autorizacion" element={<Bandeja />} />
        <Route path="/autorizacion/:id" element={<Expediente />} />
        <Route path="/flota" element={<Padron />} />
        <Route path="/tareas" element={<BandejaDeTareas />} />
        <Route path="/organizacion" element={<Puestos />} />
        <Route path="/permisos/firmar" element={<FirmaDePermisos />} />
        <Route path="/misiones/:id/salvoconducto" element={<Salvoconducto />} />

        {/* A esto resuelve el QR del papel. El parámetro es opcional: quien no pudo
            escanear entra sin él y teclea el código corto que anotó. */}
        <Route path="/verificar" element={<Verificacion />} />
        <Route path="/verificar/:codigo" element={<Verificacion />} />
        <Route path="/titulos" element={<Titulos />} />
        <Route path="/flota/:id" element={<ExpedienteDeVehiculo />} />
        {/* La matriz va ANTES que el detalle: si no, `/motoristas/matriz` entraría por
            `:id` y buscaría un motorista llamado «matriz». */}
        <Route path="/motoristas/matriz" element={<MatrizDeLicencias />} />
        <Route path="/motoristas" element={<PadronDeMotoristas />} />
        {/* Las 138 del inventario. La que existe abre su ruta real desde el mapa; la que no,
            cae acá y dice POR QUÉ no está. Ninguna queda sin destino. */}
        <Route path="/pantallas" element={<Mapa />} />
        <Route path="/pantallas/:id" element={<EnDesarrollo />} />
        <Route path="/prestamos" element={<PrestamosPantalla />} />
        <Route path="/combustible" element={<Fondos />} />
        <Route path="/peajes" element={<Peajes />} />
        <Route path="/ingreso" element={<Ingreso />} />
        <Route path="/inicio" element={<InicioDelPuesto />} />
        <Route path="/buscar" element={<Buscador />} />
        <Route path="/bloqueos" element={<Bloqueos />} />
        <Route path="/solicitudes" element={<MisSolicitudes />} />
        <Route path="/folios" element={<Folios />} />
        <Route path="/conflictos" element={<Conflictos />} />
        <Route path="/dispositivos" element={<Dispositivos />} />
        <Route path="/espejos" element={<Espejos />} />
        <Route path="/manifiesto" element={<Manifiesto />} />
        <Route path="/manifiesto/campos" element={<CamposDelManifiesto />} />
        <Route path="/manifiesto/accesos" element={<Accesos />} />
        <Route path="/habeas-data" element={<HabeasData />} />
        <Route path="/transparencia" element={<Transparencia />} />
        <Route path="/parametros/cargar" element={<CargarParametro />} />
        <Route path="/parametros/aprobar" element={<Aprobaciones />} />
        <Route path="/salud" element={<Salud />} />
        <Route path="/seguimiento" element={<TableroDeSeguimiento />} />
        <Route path="/seguimiento/:id" element={<EnRuta />} />
        <Route path="/rastro" element={<RastroDelExpediente />} />
        <Route path="/pista" element={<Pista />} />
        <Route path="/parametros-normativos" element={<ParametrosNormativos />} />
        <Route path="/intentos-bloqueados" element={<IntentosBloqueados />} />
        <Route path="/conciliacion" element={<Conciliacion />} />
        <Route path="/saldo-de-apertura" element={<SaldoDeAperturaPantalla />} />
        <Route path="/cierre-de-ejercicio" element={<CierreDeEjercicioPantalla />} />
        <Route path="/incidentes" element={<IncidentesPantalla />} />
        <Route path="/despacho" element={<Tablero />} />
        <Route path="/programacion" element={<Cola />} />
        
        <Route path="/programacion/:id" element={<Asignacion />} />
        <Route path="/cierre" element={<ColaDeCierre />} />
        <Route path="/cierre/:id" element={<Cierre />} />
        <Route
          path="/sistema-diseno"
          element={
            <Suspense fallback={<p className="tw:text-sm tw:text-tinta-mid">Cargando la vitrina…</p>}>
              <Vitrina />
            </Suspense>
          }
        />
        <Route path="*" element={<NoEncontrada />} />
      </Routes>
    </Shell>
  );
}

function migasDe(ruta: string): Miga[] {
  if (ruta.startsWith('/autorizacion/')) {
    return [{ texto: "Autorización", href: "/autorizacion" }, "Expediente"];
  }
  if (ruta === '/autorizacion') return ["Autorización"];
  if (ruta.startsWith('/programacion/')) return [{ texto: 'Programación', href: '/programacion' }, 'Asignación'];
  if (ruta === '/programacion') return ['Programación'];
  if (ruta === '/despacho') return ['Despacho'];
  if (ruta === '/tareas') return ['Tareas pendientes'];
  if (ruta === '/organizacion') return ['Organización'];
  if (ruta === '/flota') return ['Flota'];
  if (ruta.startsWith('/flota/')) return [{ texto: 'Flota', href: '/flota' }, 'Expediente del vehículo'];
  if (ruta === '/motoristas') return ['Motoristas'];
  if (ruta === '/motoristas/matriz') return [{ texto: 'Motoristas', href: '/motoristas' }, 'Matriz de licencias'];
  if (ruta === '/titulos') return ['Flota', 'Títulos de tenencia'];
  if (ruta.startsWith('/pantallas/')) {
    return [{ texto: 'Pantallas', href: '/pantallas' }, ruta.slice('/pantallas/'.length).toUpperCase()];
  }
  if (ruta === '/pantallas') return ['Pantallas'];
  if (ruta === '/prestamos') return ['Flota', 'Préstamos'];
  if (ruta === '/combustible') return ['Combustible'];
  if (ruta === '/ingreso') return ['Ingreso'];
  if (ruta === '/inicio') return ['Mi puesto'];
  if (ruta === '/buscar') return ['Mi puesto', 'Buscar expedientes'];
  if (ruta === '/bloqueos') return ['Mi puesto', 'Patrón de bloqueo'];
  if (ruta === '/solicitudes') return ['Mis solicitudes'];
  if (ruta === '/folios') return ['Control de folios'];
  if (ruta === '/conflictos') return ['Registros que no coinciden'];
  if (ruta === '/dispositivos') return ['Envíos de los equipos'];
  if (ruta === '/espejos') return ['Datos de otros sistemas'];
  if (ruta === '/manifiesto') return ['Personas externas', 'Quiénes van'];
  if (ruta === '/manifiesto/campos') return ['Personas externas', 'Qué se pregunta'];
  if (ruta === '/manifiesto/accesos') return ['Personas externas', 'Quién vio qué'];
  if (ruta === '/habeas-data') return ['Personas externas', 'Hábeas data'];
  if (ruta === '/transparencia') return ['Personas externas', 'Transparencia y depuración'];
  if (ruta === '/parametros/cargar') return ['Parámetros', 'Cargar'];
  if (ruta === '/parametros/aprobar') return ['Parámetros', 'Poner en vigencia'];
  if (ruta === '/permisos/firmar') return ['Permisos de circulación', 'Firma'];
  if (ruta.startsWith('/verificar')) return ['Verificar salvoconducto'];
  if (ruta.endsWith('/salvoconducto'))
    return [{ texto: 'Bandeja de autorización', href: '/autorizacion' }, 'Salvoconducto'];
  if (ruta === '/salud') return ['Qué está mal'];
  if (ruta === '/seguimiento') return ['Seguimiento en ruta'];
  if (ruta.startsWith('/seguimiento/'))
    return [{ texto: 'Seguimiento en ruta', href: '/seguimiento' }, 'Misión'];
  if (ruta === '/rastro') return ['Auditoría', 'Rastro del expediente'];
  if (ruta === '/pista') return ['Auditoría', 'Pista de auditoría'];
  if (ruta === '/parametros-normativos') return ['Auditoría', 'Parámetros normativos'];
  if (ruta === '/intentos-bloqueados') return ['Auditoría', 'Intentos bloqueados'];
  if (ruta === '/conciliacion') return ['Auditoría', 'Fuentes externas'];
  if (ruta === '/saldo-de-apertura') return ['Auditoría', 'Saldo de apertura'];
  if (ruta === '/cierre-de-ejercicio') return ['Auditoría', 'Cierre de ejercicio'];
  if (ruta === '/incidentes') return ['Incidentes'];
  if (ruta.startsWith('/cierre/')) return [{ texto: 'Cierre', href: '/cierre' }, 'Expediente'];
  if (ruta === '/cierre') return ['Cierre'];
  if (ruta === '/sistema-diseno') return ["Sistema de diseño"];
  return [];
}

function NoEncontrada(): ReactElement {
  return (
    <Nota tono="aviso">
      Esa dirección no corresponde a ninguna pantalla. Puede que el enlace sea de una versión
      anterior del sistema.
    </Nota>
  );
}
