import { api } from "../../../shared/services/api";
import type {
  ILoginRequest,
  ILoginResponse,
  IRegisterRequest,
  IRegisterResponse,
} from "../types";

export const login = async (
  payload: ILoginRequest,
): Promise<ILoginResponse> => {
  const response = await api.post<ILoginResponse>("/auth/login", payload);
  return response.data;
};

export const register = async (
  payload: IRegisterRequest,
): Promise<IRegisterResponse> => {
  const response = await api.post<IRegisterResponse>("/auth/register", payload);
  return response.data;
};
