export interface GetUserResponse {
    name: string;
    email: string;
    role: string;
    menus: GetMenuResponse[];
}
