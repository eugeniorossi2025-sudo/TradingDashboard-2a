import AppLayout from '@/layout/AppLayout.vue';
import { createRouter, createWebHistory } from 'vue-router';
import { adminGuard, authGuard, guestGuard } from './middleware';

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/',
            component: AppLayout,
            beforeEnter: authGuard,
            children: [
                {
                    path: '/',
                    name: 'dashboard',
                    component: () => import('@/views/Dashboard.vue')
                },
                {
                    path: '/uikit/formlayout',
                    name: 'formlayout',
                    component: () => import('@/views/uikit/FormLayout.vue')
                },
                {
                    path: '/uikit/input',
                    name: 'input',
                    component: () => import('@/views/uikit/InputDoc.vue')
                },
                {
                    path: '/uikit/button',
                    name: 'button',
                    component: () => import('@/views/uikit/ButtonDoc.vue')
                },
                {
                    path: '/uikit/table',
                    name: 'table',
                    component: () => import('@/views/uikit/TableDoc.vue')
                },
                {
                    path: '/uikit/list',
                    name: 'list',
                    component: () => import('@/views/uikit/ListDoc.vue')
                },
                {
                    path: '/uikit/tree',
                    name: 'tree',
                    component: () => import('@/views/uikit/TreeDoc.vue')
                },
                {
                    path: '/uikit/panel',
                    name: 'panel',
                    component: () => import('@/views/uikit/PanelsDoc.vue')
                },

                {
                    path: '/uikit/overlay',
                    name: 'overlay',
                    component: () => import('@/views/uikit/OverlayDoc.vue')
                },
                {
                    path: '/uikit/media',
                    name: 'media',
                    component: () => import('@/views/uikit/MediaDoc.vue')
                },
                {
                    path: '/uikit/message',
                    name: 'message',
                    component: () => import('@/views/uikit/MessagesDoc.vue')
                },
                {
                    path: '/uikit/file',
                    name: 'file',
                    component: () => import('@/views/uikit/FileDoc.vue')
                },
                {
                    path: '/uikit/menu',
                    name: 'menu',
                    component: () => import('@/views/uikit/MenuDoc.vue')
                },
                {
                    path: '/uikit/charts',
                    name: 'charts',
                    component: () => import('@/views/uikit/ChartDoc.vue')
                },
                {
                    path: '/uikit/misc',
                    name: 'misc',
                    component: () => import('@/views/uikit/MiscDoc.vue')
                },
                {
                    path: '/uikit/timeline',
                    name: 'timeline',
                    component: () => import('@/views/uikit/TimelineDoc.vue')
                },
                {
                    path: '/blocks',
                    name: 'blocks',
                    meta: {
                        breadcrumb: ['Prime Blocks', 'Free Blocks']
                    },
                    component: () => import('@/views/utilities/Blocks.vue')
                },
                {
                    path: '/pages/empty',
                    name: 'empty',
                    component: () => import('@/views/pages/Empty.vue')
                },
                // Admin only routes
                {
                    path: '/pages/pc-configuration',
                    name: 'pc-configuration',
                    beforeEnter: adminGuard,
                    meta: { requiresAdmin: true },
                    component: () => import('@/views/pages/PCManagment.vue')
                },
                {
                    path: '/pages/log',
                    name: 'log',
                    beforeEnter: adminGuard,
                    meta: { requiresAdmin: true },
                    component: () => import('@/views/pages/Log.vue')
                },
                {
                    path: '/pages/configurations',
                    name: 'configuration',
                    beforeEnter: adminGuard,
                    meta: { requiresAdmin: true },
                    component: () => import('@/views/pages/Configuration.vue')
                },
                {
                    path: '/pages/user',
                    name: 'user',
                    beforeEnter: adminGuard,
                    meta: { requiresAdmin: true },
                    component: () => import('@/views/pages/User.vue')
                },
                {
                    path: '/pages/roles-permissions',
                    name: 'roles-permissions',
                    beforeEnter: adminGuard,
                    meta: { requiresAdmin: true },
                    component: () => import('@/views/pages/RolesPermissions.vue')
                },
                {
                    path: '/pages/console',
                    name: 'console',
                    beforeEnter: adminGuard,
                    meta: { requiresAdmin: true },
                    component: () => import('@/views/pages/Console.vue')
                },
                {
                    path: '/pages/bot-sessions',
                    name: 'bot-sessions',
                    beforeEnter: adminGuard,
                    meta: { requiresAdmin: true },
                    component: () => import('@/views/pages/BotSessions.vue')
                },
                {
                    path: '/pages/roles-permissions',
                    name: 'roles-permissions',
                    beforeEnter: adminGuard,
                    meta: { requiresAdmin: true },
                    component: () => import('@/views/pages/RolesPermissions.vue')
                }
            ]
        },
        // Public routes
        {
            path: '/landing',
            name: 'landing',
            component: () => import('@/views/pages/Landing.vue')
        },
        {
            path: '/client/desktop',
            name: 'client-desktop',
            beforeEnter: authGuard,
            component: () => import('@/views/client/ClientDesktop.vue')
        },
        {
            path: '/client/mobile',
            name: 'client-mobile',
            beforeEnter: authGuard,
            component: () => import('@/views/mobile/ClientMobile.vue')
        },
        {
            path: '/admin/mobile-live',
            name: 'admin-mobile-live',
            beforeEnter: adminGuard,
            meta: { requiresAdmin: true },
            component: () => import('@/views/mobile/AdminMobileLive.vue')
        },
        {
            path: '/pages/notfound',
            name: 'notfound',
            component: () => import('@/views/pages/NotFound.vue')
        },

        // Auth routes (only for non-authenticated users)
        {
            path: '/auth/login',
            name: 'login',
            beforeEnter: guestGuard,
            component: () => import('@/views/pages/auth/Login.vue')
        },
        {
            path: '/auth/access',
            name: 'accessDenied',
            component: () => import('@/views/pages/auth/Access.vue')
        },
        {
            path: '/auth/error',
            name: 'error',
            component: () => import('@/views/pages/auth/Error.vue')
        },
        // 403 Forbidden - Access Denied
        {
            path: '/auth/access-denied',
            name: 'access-denied',
            component: () => import('@/views/pages/auth/AccessDenied.vue')
        }
    ]
});

export default router;
