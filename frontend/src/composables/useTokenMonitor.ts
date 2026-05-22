import { TokenService } from '@/service/TokenService';
import { formatTimeRemaining, getTokenTimeRemaining, isTokenExpired, isTokenExpiringSoon } from '@/utils/tokenUtils';
import { useToast } from 'primevue/usetoast';
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';

/**
 * Composable per monitorare la scadenza del token
 */
export function useTokenMonitor() {
    const router = useRouter();
    const toast = useToast();
    
    const timeRemaining = ref(0);
    const isExpired = ref(false);
    const isExpiringSoon = ref(false);
    const warningShown = ref(false);
    
    let intervalId: number | null = null;

    const timeRemainingFormatted = computed(() => formatTimeRemaining(timeRemaining.value));

    const checkToken = () => {
        const token = TokenService.getToken();
        
        if (!token) {
            isExpired.value = true;
            timeRemaining.value = 0;
            return;
        }

        isExpired.value = isTokenExpired(token);
        isExpiringSoon.value = isTokenExpiringSoon(token, 50000);
        timeRemaining.value = getTokenTimeRemaining(token);

        // Token scaduto
        if (isExpired.value) {
            console.warn('Token scaduto - Logout automatico');
            handleTokenExpired();
            return;
        }
    };

    const handleTokenExpired = () => {
        TokenService.clearAll();
        
        const currentPath = window.location.pathname + window.location.search;
        if (!currentPath.includes('/auth/')) {
            TokenService.setRedirectPath(currentPath);
        }

        router.push({ name: 'login', query: { expired: 'true' } });
    };


    return {
        timeRemaining,
        timeRemainingFormatted,
        isExpired,
        isExpiringSoon,
        checkToken
    };
}
