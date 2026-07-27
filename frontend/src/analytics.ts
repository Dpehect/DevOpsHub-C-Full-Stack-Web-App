export type AnalyticsOverview = {
  projectHealth: { score:number; grade:string; status:string; factors:{name:string;value:number;target:number;weight:number;unit:string}[] };
  metrics:{key:string;label:string;value:string;delta:string;direction:string}[];
  deliveryTrend:{label:string;value:number;secondaryValue:number}[];
  reliabilityTrend:{label:string;value:number;secondaryValue:number}[];
  teamLoad:{member:string;assigned:number;completed:number;incidents:number;capacityPercent:number}[];
  risks:{severity:string;title:string;description:string;area:string}[];
  generatedAtUtc:string;
};
