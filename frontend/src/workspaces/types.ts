export type WorkspaceRole = 'Viewer' | 'Member' | 'Manager' | 'Admin' | 'Owner';
export interface WorkspaceSummary { id:string; name:string; slug:string; description?:string; currentUserRole:WorkspaceRole; memberCount:number }
export interface WorkspaceMember { id:string; userId:string; displayName:string; email:string; role:WorkspaceRole; joinedAt:string }
export interface WorkspaceInvitation { id:string; email:string; role:WorkspaceRole; expiresAt:string; isActive:boolean }
export interface WorkspaceDetails { workspace:WorkspaceSummary; members:WorkspaceMember[]; invitations:WorkspaceInvitation[] }
