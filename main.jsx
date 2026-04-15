import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
import { ConfigProvider } from 'antd';

<ConfigProvider theme={{ token: {colorPrimary: '#fa541c'} }}> 
  <App />
</ConfigProvider>