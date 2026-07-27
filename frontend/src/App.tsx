import FilesPage from './pages/FilesPage';
import CalendarPage from './pages/CalendarPage';
import ExecutiveAnalyticsPage from './pages/ExecutiveAnalyticsPage';
import WorkflowPage from './pages/WorkflowPage';
import SearchPage from './pages/SearchPage'
import AdminPage from './pages/AdminPage'
import AnalyticsPage from './pages/AnalyticsPage'
import DocumentationPage from './pages/DocumentationPage'
import { NotificationCenter } from './notifications/NotificationCenter';
import IncidentPage from './pages/IncidentPage'
import ObservabilityPage from './pages/ObservabilityPage'
import PipelinePage from './pages/PipelinePage'
import RepositoryPage from './pages/RepositoryPage'
import ProjectBoardPage from './pages/ProjectBoardPage'
import { useState } from 'react';
import { api, getSession, setSession } from './api/client';
import type { AuthResponse } from './auth/types';
import { AuthPage } from './pages/AuthPage';
import { DashboardPage } from './pages/DashboardPage';
import { WorkspacePage } from './pages/WorkspacePage';

export function App() {
  const [session, updateSession] = useState<AuthResponse | null>(() => getSession());
  const [page, setPage] = useState<'dashboard'|'workspaces'|'board'|'repositories'|'pipelines'|'incidents'|'observability'|'documentation'|'analytics'|'admin'|'search'|'files'|'calendar'|'executive'|'workflow'>('dashboard');
  async function logout() { if (session) await api.logout(session.refreshToken).catch(() => undefined); setSession(null); updateSession(null); }
  if (!session) return <AuthPage onAuthenticated={updateSession}/>;
  const wrap = (content: React.ReactNode) => <>{content}<NotificationCenter/></>;
  if (page === 'workspaces') return wrap(<WorkspacePage onBack={()=>setPage('dashboard')}/>);
  if (page === 'board') return wrap(<ProjectBoardPage/>);
  if (page === 'repositories') return wrap(<RepositoryPage/>);
  if (page === 'pipelines') return wrap(<PipelinePage/>);
  if (page === 'incidents') return wrap(<IncidentPage onBack={()=>setPage('dashboard')}/>);
  if (page === 'observability') return wrap(<ObservabilityPage onBack={()=>setPage('dashboard')}/>);
  if (page === 'documentation') return wrap(<DocumentationPage onBack={()=>setPage('dashboard')}/>);
  if (page === 'analytics') return wrap(<AnalyticsPage onBack={()=>setPage('dashboard')}/>);
  if (page === 'search') return wrap(<SearchPage onBack={()=>setPage('dashboard')}/>);
  if (page === 'files') return wrap(<FilesPage onBack={()=>setPage('dashboard')}/>);
  if (page === 'calendar') return wrap(<CalendarPage onBack={()=>setPage('dashboard')}/>);
  if (page === 'executive') return wrap(<ExecutiveAnalyticsPage onBack={()=>setPage('dashboard')}/>);
  if (page === 'workflow') return wrap(<WorkflowPage onBack={()=>setPage('dashboard')}/>);
  if (page === 'admin') return wrap(<AdminPage onBack={()=>setPage('dashboard')}/>);
  return wrap(<DashboardPage session={session} onLogout={logout} onOpenWorkspaces={()=>setPage('workspaces')} onOpenProjects={()=>setPage('board')} onOpenRepositories={()=>setPage('repositories')} onOpenPipelines={()=>setPage('pipelines')} onOpenIncidents={()=>setPage('incidents')} onOpenObservability={()=>setPage('observability')} onOpenDocumentation={()=>setPage('documentation')} onOpenAnalytics={()=>setPage('analytics')} onOpenAdmin={()=>setPage('admin')} onOpenSearch={()=>setPage('search')} onOpenFiles={()=>setPage('files')} onOpenCalendar={()=>setPage('calendar')} onOpenExecutive={()=>setPage('executive')} onOpenWorkflow={()=>setPage('workflow')}/>);
}
