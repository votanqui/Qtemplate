import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import './index.css'
import { ThemeProvider } from './context/ThemeContext.jsx'
import { LangProvider } from './context/Langcontext.jsx'   // ← THÊM
import { ToastProvider } from './components/ui/index.jsx'
import { saveAffiliateCode } from './utils/affiliate.js'

// Đọc ?ref= từ URL và lưu cookie 30 ngày
const params = new URLSearchParams(window.location.search);
const refCode = params.get('ref');
if (refCode) saveAffiliateCode(refCode);

ReactDOM.createRoot(document.getElementById('root')).render(
  <ThemeProvider>
    <LangProvider>          {/* ← THÊM */}
      <ToastProvider>
        <App />
      </ToastProvider>
    </LangProvider>         {/* ← THÊM */}
  </ThemeProvider>
)