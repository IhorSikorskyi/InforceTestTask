export interface ShortUrlInfoResponse {
  urlId: number;
  longUrl: string;
  shortUrl: string;
  code: string;
  createdBy: string;
  createdByUserId: number;
  createdDate: Date;
}
