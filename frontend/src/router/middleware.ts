import { AuthConstants } from '@/constants/AuthConstants';
import { AuthService } from '@/service/AuthService';
import { NavigationGuardNext, RouteLocationNormalized } from 'vue-router';

/**
 * Middleware per verificare l'autenticazione
 */
export const authGuard = (
    to: RouteLocationNormalized,
    from: RouteLocationNormalized,
    next: NavigationGuardNext
) => {
    if (AuthService.isAuthenticated.value) {
        next();
    } else {
        next({
            name: 'login',
            query: { redirect: to.fullPath },
        });
    }
};

const isMobileViewport = () => {
    if (typeof window === 'undefined') return false;
    return window.matchMedia('(max-width: 768px), (pointer: coarse)').matches;
};

export const mobileHomeRedirectGuard = (
    to: RouteLocationNormalized,
    from: RouteLocationNormalized,
    next: NavigationGuardNext
) => {
    if (!AuthService.isAuthenticated.value || !isMobileViewport()) {
        next();
        return;
    }

    next({
        name: AuthService.isAdmin.value ? 'admin-mobile-live' : 'client-mobile',
    });
};

/**
 * Middleware per verificare il ruolo admin
 */
export const adminGuard = (
    to: RouteLocationNormalized,
    from: RouteLocationNormalized,
    next: NavigationGuardNext
) => {
    if (!AuthService.isAuthenticated.value) {
        next({
            name: 'login',
            query: { redirect: to.fullPath },
        });
    } else if (AuthService.checkPolicy(AuthConstants.Policies.RequireAdmin)) {
        next();
    } else {
        // Utente autenticato ma senza permessi admin - mostra 403
        next({ name: 'access-denied' });
    }
};

/**
 * Middleware per verificare il ruolo Bot Operator o Admin
 */
export const botOperatorGuard = (
    to: RouteLocationNormalized,
    from: RouteLocationNormalized,
    next: NavigationGuardNext
) => {
    if (!AuthService.isAuthenticated.value) {
        next({
            name: 'login',
            query: { redirect: to.fullPath },
        });
    } else if (AuthService.checkPolicy(AuthConstants.Policies.RequireAdminOrBotOperator)) {
        next();
    } else {
        // Utente autenticato ma senza permessi - mostra 403
        next({ name: 'access-denied' });
    }
};

/**
 * Middleware generico per verificare una policy
 */
export const policyGuard = (policy: string) => {
    return (
        to: RouteLocationNormalized,
        from: RouteLocationNormalized,
        next: NavigationGuardNext
    ) => {
        if (!AuthService.isAuthenticated.value) {
            next({
                name: 'login',
                query: { redirect: to.fullPath },
            });
        } else if (AuthService.checkPolicy(policy)) {
            next();
        } else {
            // Utente autenticato ma senza permessi per la policy - mostra 403
            next({ name: 'access-denied' });
        }
    };
};

/**
 * Middleware per verificare uno o più permessi specifici
 */
export const permissionGuard = (...permissions: string[]) => {
    return (
        to: RouteLocationNormalized,
        from: RouteLocationNormalized,
        next: NavigationGuardNext
    ) => {
        if (!AuthService.isAuthenticated.value) {
            next({
                name: 'login',
                query: { redirect: to.fullPath },
            });
        } else if (AuthService.hasAnyPermission(...permissions)) {
            next();
        } else {
            // Utente autenticato ma senza permessi - mostra 403
            next({ name: 'access-denied' });
        }
    };
};

/**
 * Middleware per verificare un ruolo specifico
 */
export const roleGuard = (...roles: string[]) => {
    return (
        to: RouteLocationNormalized,
        from: RouteLocationNormalized,
        next: NavigationGuardNext
    ) => {
        if (!AuthService.isAuthenticated.value) {
            next({
                name: 'login',
                query: { redirect: to.fullPath },
            });
        } else if (roles.some(role => AuthService.hasRole(role))) {
            next();
        } else {
            // Utente autenticato ma senza ruolo richiesto - mostra 403
            next({ name: 'access-denied' });
        }
    };
};

/**
 * Middleware per route pubbliche (solo per utenti non autenticati)
 * Es: pagina login, se sei già loggato vai alla dashboard
 */
export const guestGuard = (
    to: RouteLocationNormalized,
    from: RouteLocationNormalized,
    next: NavigationGuardNext
) => {
    if (AuthService.isAuthenticated.value) {
        next({ name: 'dashboard' });
    } else {
        next();
    }
};
