export interface Log {
  id: number;
  json: any;            // JSON molto grande e annidato
  datetime: string;     // formato dd/MM/yyyy HH:mm:ss (come il tuo input)
  notes: string;        // può essere vuoto
  margine: number;      // decimale
}
