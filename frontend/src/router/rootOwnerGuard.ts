import { AuthService } from '@/service/AuthService';
import { RootOwnerService } from '@/service/RootOwnerService';
import { NavigationGuardNext, RouteLocationNormalized } from 'vue-router';

/**
 * Hidden owner console — not linked from menu. Requires IsRootOwner.
 */
export const rootOwnerGuard = async (
    to: RouteLocationNormalized,
    from: RouteLocationNormalized,
    next: NavigationGuardNext
) => {
    if (!AuthService.isAuthenticated.value) {
        next({ name: 'login', query: { redirect: to.fullPath } });
        return;
    }

    if (AuthService.isRootOwner.value) {
        next();
        return;
    }

    try {
        const me = await RootOwnerService.getMe();
        if (me.isRootOwner) {
            next();
            return;
        }
    } catch {
        /* fall through */
    }

    next({
        name: 'access-denied',
        query: { code: 'ROOT_OWNER_ONLY' }
    });
};
