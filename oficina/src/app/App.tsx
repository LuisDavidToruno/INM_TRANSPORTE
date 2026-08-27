import { Suspense, lazy } from 'react';
import type { ReactElement } from 'react';
import { Navigate, Route, Routes, useLocation } from "react-router";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useQuery } from '@tanstack/react-query';
import { CalendarCog, ClipboardCheck, Palette } from 'lucide-react';

import { Avisos, Nota, Shell, avisar } from '../ui';
import type { GrupoNav, Miga } from '../ui';
import logo from '../marca/argos/logo.png';

// La vitrina es documentación del sistema de diseño, no la aplicación. Se carga
// aparte para que quien entra a autorizar un expediente no descargue la galería.
const Vitrina = lazy(() => import('../vitrina/Vitrina'));
import Bandeja from '../modulos/M06_Autorizacion/Bandeja';
import Expediente from '../modulos/M06_Autorizacion/Expediente';
import Asignacion from '../modulos/M07_Programacion/Asignacion';
import { bandejaDeAutorizacion, origenDeDatos } from '../api/misiones';

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

const USUARIO = { nombre: 'Rolando Discua', rol: 'Jefatura Inmediata · ACT-03' };

const MARCA = { logo, nombre: 'SIGTI', bajada: 'Transporte institucional' };

export default function App(): ReactElement {
  return (
    <QueryClientProvider client={cliente}>
      <Interior />
      <Avisos />
    </QueryClientProvider>
  );
}

function Interior(): ReactElement {
  const { pathname } = useLocation();

  // El contador del riel sale del mismo dato que la bandeja, no de un conteo
  // aparte: dos fuentes para el mismo número divergen el día que una falla.
  const { data } = useQuery({
    queryKey: ['bandeja-autorizacion'],
    queryFn: bandejaDeAutorizacion,
  });

  const grupos: GrupoNav[] = [
    {
      titulo: 'M-06 Solicitud y autorización',
      items: [
        {
          texto: 'Bandeja de autorización',
          icono: <ClipboardCheck />,
          href: '/autorizacion',
          contador: data?.length,
          accionable: true,
        },
      ],
    },
    {
      titulo: 'M-07 Programación y despacho',
      items: [{ texto: 'Programar misión', icono: <CalendarCog />, href: '/programacion' }],
    },
    { items: [{ texto: 'Sistema de diseño', icono: <Palette />, href: '/sistema-diseno' }] },
  ];

  return (
    <Shell
      marca={MARCA}
      usuario={USUARIO}
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
        <Route path="/programacion" element={<Asignacion />} />
        <Route
          path="/sistema-diseno"
          element={
            <Suspense fallback={<p className="tw:text-sm tw:text-[var(--txt-2)]">Cargando la vitrina…</p>}>
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
  if (ruta === '/programacion') return ['Programación'];
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
