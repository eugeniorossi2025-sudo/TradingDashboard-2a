import { computed, onMounted, reactive, watch } from 'vue';

const LOCAL_STORAGE_KEY = 'app-configurator-layout';
const LOCAL_STORAGE_DARK_KEY = 'app-configurator-dark';

const layoutConfig = reactive({
    preset: 'Aura',
    primary: 'emerald',
    surface: null,
    darkTheme: false,
    menuMode: 'static'
});

// Carica la configurazione da localStorage se presente
const loadConfigFromStorage = () => {
    const saved = localStorage.getItem(LOCAL_STORAGE_KEY);
    if (saved) {
        try {
            const parsed = JSON.parse(saved);
            Object.assign(layoutConfig, parsed);
        } catch {
            // ignore parse error
        }
    }
    const savedDark = localStorage.getItem(LOCAL_STORAGE_DARK_KEY);
    if (savedDark !== null) {
        try {
            layoutConfig.darkTheme = JSON.parse(savedDark);
        } catch {
            // ignore parse error
        }
    }
};

loadConfigFromStorage();

// Salva ogni volta che layoutConfig cambia
watch(
    layoutConfig,
    (val) => {
        localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(val));
        localStorage.setItem(LOCAL_STORAGE_DARK_KEY, JSON.stringify(val.darkTheme));
    },
    { deep: true }
);

const layoutState = reactive({
    staticMenuDesktopInactive: false,
    overlayMenuActive: false,
    profileSidebarVisible: false,
    configSidebarVisible: false,
    staticMenuMobileActive: false,
    menuHoverActive: false,
    activeMenuItem: null
});

export function useLayout() {
    onMounted(() => {
        if (layoutConfig.darkTheme) {
            document.documentElement.classList.add('app-dark');
        } else {
            document.documentElement.classList.remove('app-dark');
        }
    });
    const setActiveMenuItem = (item) => {
        layoutState.activeMenuItem = item.value || item;
    };

    const toggleDarkMode = () => {
        if (!document.startViewTransition) {
            executeDarkModeToggle();

            return;
        }

        document.startViewTransition(() => executeDarkModeToggle(event));
    };

    const executeDarkModeToggle = () => {
        layoutConfig.darkTheme = !layoutConfig.darkTheme;
        if (layoutConfig.darkTheme) {
            document.documentElement.classList.add('app-dark');
        } else {
            document.documentElement.classList.remove('app-dark');
        }
    };

    const toggleMenu = () => {
        if (layoutConfig.menuMode === 'overlay') {
            layoutState.overlayMenuActive = !layoutState.overlayMenuActive;
        }

        if (window.innerWidth > 991) {
            layoutState.staticMenuDesktopInactive = !layoutState.staticMenuDesktopInactive;
        } else {
            layoutState.staticMenuMobileActive = !layoutState.staticMenuMobileActive;
        }
    };

    const isSidebarActive = computed(() => layoutState.overlayMenuActive || layoutState.staticMenuMobileActive);

    const isDarkTheme = computed(() => layoutConfig.darkTheme);

    const getPrimary = computed(() => layoutConfig.primary);

    const getSurface = computed(() => layoutConfig.surface);

    return {
        layoutConfig,
        layoutState,
        toggleMenu,
        isSidebarActive,
        isDarkTheme,
        getPrimary,
        getSurface,
        setActiveMenuItem,
        toggleDarkMode
    };
}
