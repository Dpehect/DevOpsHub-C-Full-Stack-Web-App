export type ServiceStatus='Operational'|'Degraded'|'PartialOutage'|'MajorOutage'|'Maintenance';
export type IncidentStatus='Investigating'|'Identified'|'Monitoring'|'Resolved';
export type IncidentSeverity='Sev1'|'Sev2'|'Sev3'|'Sev4';
export interface Service {id:string;name:string;slug:string;description:string;status:ServiceStatus;availabilityPercent:number;targetSlaMinutes:number;openIncidentCount:number}
export interface IncidentUpdate {id:string;status:IncidentStatus;message:string;author:string;createdAtUtc:string}
export interface Incident {id:string;number:number;title:string;summary:string;severity:IncidentSeverity;status:IncidentStatus;commander:string;startedAtUtc:string;resolvedAtUtc?:string;slaDueAtUtc:string;isSlaBreached:boolean;serviceName:string;updates:IncidentUpdate[]}
export interface IncidentDashboard {activeIncidents:number;sev1Count:number;mttaMinutes:number;mttrMinutes:number;slaCompliancePercent:number;services:Service[];incidents:Incident[]}
