export interface ResultDto<T> {
  data?: T;
  errorMessage?: string;
}

export interface UserRequestDto {
  email: string;
  password: string;
}

export interface UserReturnDto {
  email: string;
}

export interface TokensDto {
  authToken: string;
  refreshToken: string;
}
