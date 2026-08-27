import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router';

import './estilos/index.css';
import App from './app/App';
import { TemaProvider, aplicarPreferenciaGuardada } from './tema/TemaProvider';

/**
 * Entrada del bundle.
 *
 * `aplicarPreferenciaGuardada()` corre ANTES de montar React y es una red, no el
 * mecanismo principal: el tema ya lo fijó el script bloqueante de `index.html`
 * antes del primer pintado. Esto cubre el caso de que este bundle termine
 * montado en una página que no traiga ese script — ahí sin esta línea el tema
 * elegido no se restauraría.
 *
 * `TemaProvider` envuelve TODO y no sólo el shell: desde que `useTema()` es un
 * contexto, un componente que lo llame fuera del proveedor lanza al montar.
 */
aplicarPreferenciaGuardada();

const raiz = document.getElementById('raiz');
if (!raiz) throw new Error('Falta <div id="raiz"> en index.html.');

createRoot(raiz).render(
  <StrictMode>
    <TemaProvider>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </TemaProvider>
  </StrictMode>,
);
