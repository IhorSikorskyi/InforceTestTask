export interface ShortUrlTableRequest {
  userId: number;
  longUrl: string;
}

export interface ShortUrlInfoRequest {
  urlId: number;
}

export interface ShortUrlDeleteRequest {
  userId: number;
  urlId: number;
}
