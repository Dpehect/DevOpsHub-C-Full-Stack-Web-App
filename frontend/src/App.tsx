import { Activity, GitBranch, Server, ShieldCheck } from 'lucide-react';

const metrics = [
  { label: 'Active projects', value: '12', icon: GitBranch },
  { label: 'Healthy services', value: '24/26', icon: Server },
  { label: 'Pipeline success', value: '94.8%', icon: Activity },
  { label: 'Open incidents', value: '3', icon: ShieldCheck },
];

export function App() {
  return (
    <main className="shell">
      <aside className="sidebar">
        <div className="brand">DevOpsHub</div>
        <nav>
          <a className="active">Overview</a>
          <a>Projects</a>
          <a>Pipelines</a>
          <a>Incidents</a>
          <a>Repositories</a>
          <a>Audit log</a>
        </nav>
      </aside>
      <section className="content">
        <header>
          <div>
            <p className="eyebrow">Engineering workspace</p>
            <h1>Operational overview</h1>
          </div>
          <button>New project</button>
        </header>
        <section className="grid">
          {metrics.map(({ label, value, icon: Icon }) => (
            <article className="card" key={label}>
              <Icon size={20} />
              <span>{label}</span>
              <strong>{value}</strong>
            </article>
          ))}
        </section>
        <section className="panel">
          <div>
            <p className="eyebrow">System status</p>
            <h2>All core services operational</h2>
          </div>
          <span className="status">Healthy</span>
        </section>
      </section>
    </main>
  );
}
