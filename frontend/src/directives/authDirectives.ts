import { AuthService } from '@/service/AuthService';
import type { Directive, DirectiveBinding } from 'vue';

/**
 * v-permission directive
 * Hides elements if user doesn't have required permission
 * 
 * Usage:
 * <button v-permission="'Users.Create'">Create User</button>
 * <div v-permission="['Users.Create', 'Users.Edit']">Multiple permissions (OR)</div>
 * <div v-permission.all="['Users.Create', 'Users.Edit']">All permissions required (AND)</div>
 */
export const permissionDirective: Directive = {
    mounted(el: HTMLElement, binding: DirectiveBinding) {
        checkPermission(el, binding);
    },
    updated(el: HTMLElement, binding: DirectiveBinding) {
        checkPermission(el, binding);
    }
};

function checkPermission(el: HTMLElement, binding: DirectiveBinding) {
    const permissions = Array.isArray(binding.value) ? binding.value : [binding.value];
    const requireAll = binding.modifiers.all;

    let hasPermission = false;

    if (requireAll) {
        hasPermission = AuthService.hasAllPermissions(...permissions);
    } else {
        hasPermission = AuthService.hasAnyPermission(...permissions);
    }

    if (!hasPermission) {
        // Remove element from DOM
        el.style.display = 'none';
        el.setAttribute('aria-hidden', 'true');
    } else {
        el.style.display = '';
        el.removeAttribute('aria-hidden');
    }
}

/**
 * v-role directive
 * Hides elements if user doesn't have required role
 * 
 * Usage:
 * <button v-role="'Admin'">Admin Only</button>
 * <div v-role="['Admin', 'BotOperator']">Multiple roles (OR)</div>
 */
export const roleDirective: Directive = {
    mounted(el: HTMLElement, binding: DirectiveBinding) {
        checkRole(el, binding);
    },
    updated(el: HTMLElement, binding: DirectiveBinding) {
        checkRole(el, binding);
    }
};

function checkRole(el: HTMLElement, binding: DirectiveBinding) {
    const roles = Array.isArray(binding.value) ? binding.value : [binding.value];
    const hasRole = roles.some((role: string) => AuthService.hasRole(role));

    if (!hasRole) {
        el.style.display = 'none';
        el.setAttribute('aria-hidden', 'true');
    } else {
        el.style.display = '';
        el.removeAttribute('aria-hidden');
    }
}

/**
 * v-policy directive
 * Hides elements if user doesn't satisfy policy
 * 
 * Usage:
 * <button v-policy="'RequireAdmin'">Admin Only</button>
 * <div v-policy="'RequireAdminOrBotOperator'">Admin or Bot Operator</div>
 */
export const policyDirective: Directive = {
    mounted(el: HTMLElement, binding: DirectiveBinding) {
        checkPolicyDirective(el, binding);
    },
    updated(el: HTMLElement, binding: DirectiveBinding) {
        checkPolicyDirective(el, binding);
    }
};

function checkPolicyDirective(el: HTMLElement, binding: DirectiveBinding) {
    const policy = binding.value;
    const hasPolicy = AuthService.checkPolicy(policy);

    if (!hasPolicy) {
        el.style.display = 'none';
        el.setAttribute('aria-hidden', 'true');
    } else {
        el.style.display = '';
        el.removeAttribute('aria-hidden');
    }
}
