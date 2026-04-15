import React, { useState, useEffect } from 'react';
import Login from './pages/Login';
import Vendor from './pages/Vendor';
import Admin from './pages/Admin';
import UserTour from './pages/UserTour'; // <--- Import trang mới

export default function App() {
  const [page, setPage] = useState('login');

  useEffect(() => {
    if (!document.getElementById('tw-cdn')) {
      const script = document.createElement('script'); script.id = 'tw-cdn'; script.src = 'https://cdn.tailwindcss.com'; document.head.appendChild(script);
    }
    const style = document.createElement('style');
    style.innerHTML = `* { margin: 0; padding: 0; box-sizing: border-box; } html, body, #root { width: 100vw !important; height: 100vh !important; overflow: hidden !important; background-color: #020617; } .custom-scrollbar::-webkit-scrollbar { width: 6px; } .custom-scrollbar::-webkit-scrollbar-thumb { background-color: #cbd5e1; border-radius: 10px; }`;
    document.head.appendChild(style);
  }, []);

  if (page === 'vendor') return <Vendor onLogout={() => setPage('login')} />;
  if (page === 'admin') return <Admin onLogout={() => setPage('login')} />;
  if (page === 'user') return <UserTour onLogout={() => setPage('login')} />; // <--- Chuyển hướng
  
  return <Login onLogin={setPage} />;
}