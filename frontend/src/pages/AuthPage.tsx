import { useState, type FormEvent } from 'react';
import { LockKeyhole, ShieldCheck, Workflow } from 'lucide-react';
import { api, setSession } from '../api/client';
import type { AuthResponse } from '../auth/types';

type Props = { onAuthenticated: (session: AuthResponse) => void };

export function AuthPage({ onAuthenticated }: Props) {
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [email, setEmail] = useState('admin@devopshub.local');
  const [displayName, setDisplayName] = useState('Demo Admin');
  const [password, setPassword] = useState('Admin123!');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault(); setLoading(true); setError('');
    try {
      const session = mode === 'login' ? await api.login(email, password) : await api.register(email, displayName, password);
      setSession(session); onAuthenticated(session);
    } catch (err) { setError(err instanceof Error ? err.message : 'Authentication failed.'); }
    finally { setLoading(false); }
  }

  return <main className="auth-layout">
    <section className="auth-hero">
      <div className="brand-large"><Workflow size={28}/> DevOpsHub</div>
      <div className="hero-copy"><p className="eyebrow">Engineering operations workspace</p><h1>Ship reliably.<br/>Respond faster.</h1><p>Projects, pipelines, incidents and engineering activity in a single secure workspace.</p></div>
      <div className="trust-row"><span><ShieldCheck size={18}/> Local-first demo</span><span><LockKeyhole size={18}/> JWT security</span></div>
    </section>
    <section className="auth-panel">
      <form className="auth-card" onSubmit={submit}>
        <div><p className="eyebrow">Welcome</p><h2>{mode === 'login' ? 'Sign in to workspace' : 'Create your account'}</h2></div>
        {mode === 'register' && <label>Full name<input value={displayName} onChange={e => setDisplayName(e.target.value)} required minLength={2}/></label>}
        <label>Email<input type="email" value={email} onChange={e => setEmail(e.target.value)} required/></label>
        <label>Password<input type="password" value={password} onChange={e => setPassword(e.target.value)} required minLength={8}/></label>
        {error && <div className="form-error">{error}</div>}
        <button className="primary wide" disabled={loading}>{loading ? 'Please wait…' : mode === 'login' ? 'Sign in' : 'Create account'}</button>
        <button className="text-button" type="button" onClick={() => setMode(mode === 'login' ? 'register' : 'login')}>{mode === 'login' ? 'Create a new account' : 'Already have an account'}</button>
        <div className="demo-note"><strong>Demo account</strong><span>admin@devopshub.local / Admin123!</span></div>
      </form>
    </section>
  </main>;
}
