export type AdminOverview = {
  totalUsers:number; activeUsers:number; workspaces:number; openIncidents:number;
  failedPipelines:number; unreadNotifications:number;
  recentActivity:{id:string;action:string;entityType:string;userEmail?:string;createdAtUtc:string}[]
}
export type AdminUser={id:string;email:string;displayName:string;role:'Admin'|'Owner'|'Member';isActive:boolean;createdAtUtc:string;lastLoginAtUtc?:string}
export type AdminWorkspace={id:string;name:string;slug:string;members:number;projects:number;createdAtUtc:string}
export type FeatureFlag={id:string;key:string;description:string;isEnabled:boolean;updatedAtUtc:string}
export type SystemSetting={id:string;key:string;value:string;category:string;isSecret:boolean;updatedAtUtc:string}
export type HealthSnapshot={api:string;database:string;databaseSizeBytes:number;logsLast24Hours:number;errorsLast24Hours:number;checkedAtUtc:string}
