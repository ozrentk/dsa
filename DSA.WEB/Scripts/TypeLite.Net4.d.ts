
 
 

 

/// <reference path="Enums.ts" />

declare namespace DSA.WEB.Models {
	interface LoginRequest {
		Password: string;
		Username: string;
	}
	interface LoginResponse {
		Jwt: string;
	}
}


