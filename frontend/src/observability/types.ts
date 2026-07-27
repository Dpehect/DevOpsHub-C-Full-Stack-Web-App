export type PagedResult<T> = { items:T[]; total:number; page:number; pageSize:number };
export type SystemLog = { id:string; level:string; message:string; category:string; exception?:string; requestId?:string; userId?:string; path?:string; method?:string; statusCode?:number; durationMs?:number; createdAtUtc:string };
export type AuditEntry = { id:string; action:string; entityType:string; entityId?:string; userId?:string; userEmail?:string; ipAddress?:string; requestId?:string; succeeded:boolean; createdAtUtc:string };
export type ObservabilityStats = { logs24h:number; errors24h:number; audits24h:number; averageRequestMs:number; errorRatePercent:number };
