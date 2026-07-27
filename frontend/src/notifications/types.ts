export type NotificationType='Info'|'Success'|'Warning'|'Error'|'Assignment'|'Mention'|'Pipeline'|'Incident';
export interface NotificationItem{id:string;userId:string;workspaceId?:string;type:NotificationType;title:string;message:string;actionUrl?:string;source?:string;isRead:boolean;createdAtUtc:string;readAtUtc?:string}
export interface NotificationPage{items:NotificationItem[];unreadCount:number;totalCount:number}
