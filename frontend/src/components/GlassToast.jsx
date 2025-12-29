import React, { useEffect } from 'react';
import './GlassToast.css'; // Stil dosyasını oluşturacağız

const ToastItem = ({ toast, onClose }) => {
    useEffect(() => {
        const timer = setTimeout(() => {
            onClose(toast.id);
        }, 4000); // 4 saniye sonra otomatik kapan
        return () => clearTimeout(timer);
    }, [toast.id, onClose]);

    return (
        <div className={`glass-toast ${toast.type || 'info'}`}>
            <div className="toast-content">
                {toast.icon && <span className="toast-icon">{toast.icon}</span>}
                <div className="toast-message">
                    {toast.title && <div className="toast-title">{toast.title}</div>}
                    <div className="toast-body">{toast.message}</div>
                </div>
            </div>
            <button className="toast-close" onClick={() => onClose(toast.id)}>×</button>
        </div>
    );
};

export default function GlassToast({ toasts, removeToast }) {
    if (!toasts || toasts.length === 0) return null;

    return (
        <div className="glass-toast-container">
            {toasts.map(toast => (
                <ToastItem key={toast.id} toast={toast} onClose={removeToast} />
            ))}
        </div>
    );
}
