import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  // Get expected roles from route data
  const expectedRoles = route.data?.['roles'] as Array<string>;
  const userRole = authService.getRole();

  if (!expectedRoles || expectedRoles.length === 0) {
    return true; // No specific roles required
  }

  if (userRole && expectedRoles.includes(userRole)) {
    return true;
  }

  // Not authorized
  router.navigate(['/app/dashboard']);
  return false;
};
