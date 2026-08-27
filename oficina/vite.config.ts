import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwind from '@tailwindcss/vite';

/**
 * LOKI se sirve solo: es una SPA estática, sin backend.
 *
 * Un proyecto que nazca de acá y tenga que montarse dentro de otra aplicación
 * (por ejemplo servido por .NET desde `wwwroot/dist`) cambia `base` y `outDir`
 * acá y nada más — ningún componente sabe dónde está publicado.
 *
 * ── Sobre partir el bundle: se intentó, se midió, y se dejó como está ────────
 *
 * La tentación es agregar `build.rollupOptions.output.manualChunks` para separar
 * React y los iconos «y que la caché no se invalide entera en cada despliegue».
 * Se probó acá y **empeoró la primera carga de 1.159 kB a 1.727 kB**, por dos
 * motivos que no son obvios:
 *
 *   1. Un `return 'vendor'` como caso final **se traga ECharts**, que entra por
 *      `import()` dinámico. Su trozo pasó a 0,14 kB —un cascarón— y los 568 kB
 *      reales se mudaron al trozo estático que baja todo el mundo. Se perdió
 *      justo la propiedad que se buscaba.
 *   2. Forzar `lucide-react` a un trozo propio **rompe el sacudido del árbol**:
 *      Rollup ya no puede descartar los iconos que nadie usa, y la librería pasó
 *      de unos pocos kB a 648 kB.
 *
 * Lo que sí funciona ya está hecho y no necesita configuración: el `import()`
 * dinámico de `graficos/echarts.ts` hace que Rollup emita ECharts en su propio
 * trozo y **sólo lo baje cuando una gráfica se monta**.
 *
 * ⇒ Si algún día hace falta partirlo de verdad, el camino es cargar las RUTAS con
 *   `lazy()`, no listar dependencias a mano. Y se mide antes y después: acá la
 *   intuición se equivocó por 568 kB.
 */
export default defineConfig({
  plugins: [react(), tailwind()],
  server: { port: 5180, open: true },
});
