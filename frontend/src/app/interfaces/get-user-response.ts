import { GetMenuResponse } from "./get-menu-response";

export interface GetUserResponse {
    name: string;
    email: string;
    role: string;
    menus: GetMenuResponse[];
}
