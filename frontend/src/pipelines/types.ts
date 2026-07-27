export type PipelineStatus='Queued'|'Running'|'Succeeded'|'Failed'|'Cancelled'
export type StageStatus='Pending'|'Running'|'Succeeded'|'Failed'|'Skipped'
export interface Stage{id:string;name:string;order:number;status:StageStatus;durationSeconds:number;log:string}
export interface Deployment{id:string;environment:'Development'|'Staging'|'Production';status:PipelineStatus;version:string;url:string;deployedAt:string;deployedBy:string}
export interface PipelineRun{id:string;pipelineDefinitionId:string;number:number;status:PipelineStatus;trigger:string;branch:string;commitSha:string;commitMessage:string;triggeredBy:string;queuedAt:string;startedAt?:string;completedAt?:string;stages:Stage[];deployments:Deployment[]}
export interface PipelineDefinition{id:string;repositoryId:string;name:string;branch:string;isActive:boolean;totalRuns:number;successRate:number;updatedAt:string}
