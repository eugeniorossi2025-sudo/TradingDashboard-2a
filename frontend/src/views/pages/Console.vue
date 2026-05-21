<script setup>
import { CommandService } from '@/service/CommandService';
import { useToast } from 'primevue/usetoast';
import { nextTick, onMounted, ref } from 'vue';

const toast = useToast();

const commandInput = ref('');
const commandHistory = ref([]);
const historyIndex = ref(-1);
const executing = ref(false);
const terminalOutput = ref(null);
const availableCommands = ref([]);
const filteredCommands = ref([]);
const showSuggestions = ref(false);

onMounted(() => {
    availableCommands.value = CommandService.getAvailableCommands();

    // Load history from localStorage
    const savedHistory = localStorage.getItem('commandHistory');
    if (savedHistory) {
        try {
            commandHistory.value = JSON.parse(savedHistory);
        } catch (error) {
            console.error('Error loading command history:', error);
        }
    }
});

// 🔹 EXECUTE COMMAND
const executeCommand = async () => {
    const cmd = commandInput.value.trim();
    if (!cmd || executing.value) return;

    executing.value = true;

    // Add command to history display
    const historyEntry = {
        id: Date.now().toString(),
        command: cmd,
        timestamp: new Date(),
        output: 'Executing...',
        success: false
    };

    commandHistory.value.push(historyEntry);
    commandInput.value = '';
    historyIndex.value = -1;
    showSuggestions.value = false;

    await scrollToBottom();

    try {
        const result = await CommandService.executeCommand(cmd);

        // Update history entry with result
        historyEntry.output = result.output;
        historyEntry.success = result.success;

        if (!result.success) {
            toast.add({
                severity: 'error',
                summary: 'Command Failed',
                detail: result.output,
                life: 3000
            });
        }
    } catch (error) {
        historyEntry.output = `Error: ${error.message}`;
        historyEntry.success = false;

        toast.add({
            severity: 'error',
            summary: 'Execution Error',
            detail: error.message,
            life: 3000
        });
    } finally {
        executing.value = false;
        await scrollToBottom();

        // Save history to localStorage
        saveHistory();
    }
};

// 🔹 HANDLE KEY NAVIGATION
const handleKeyDown = (event) => {
    // Arrow Up - Previous command
    if (event.key === 'ArrowUp') {
        event.preventDefault();
        const commands = commandHistory.value.map(h => h.command);
        if (commands.length > 0) {
            if (historyIndex.value < commands.length - 1) {
                historyIndex.value++;
                commandInput.value = commands[commands.length - 1 - historyIndex.value];
            }
        }
    }

    // Arrow Down - Next command
    else if (event.key === 'ArrowDown') {
        event.preventDefault();
        if (historyIndex.value > 0) {
            historyIndex.value--;
            const commands = commandHistory.value.map(h => h.command);
            commandInput.value = commands[commands.length - 1 - historyIndex.value];
        } else if (historyIndex.value === 0) {
            historyIndex.value = -1;
            commandInput.value = '';
        }
    }

    // Tab - Autocomplete
    else if (event.key === 'Tab' && filteredCommands.value.length > 0) {
        event.preventDefault();
        commandInput.value = filteredCommands.value[0];
        showSuggestions.value = false;
    }
};

// 🔹 FILTER COMMANDS FOR AUTOCOMPLETE
const onInputChange = () => {
    const input = commandInput.value.toLowerCase().trim();

    if (input.length > 0) {
        filteredCommands.value = availableCommands.value.filter(cmd =>
            cmd.toLowerCase().startsWith(input)
        );
        showSuggestions.value = filteredCommands.value.length > 0;
    } else {
        showSuggestions.value = false;
        filteredCommands.value = [];
    }
};

// 🔹 SELECT SUGGESTION
const selectSuggestion = (command) => {
    commandInput.value = command;
    showSuggestions.value = false;
};

// 🔹 CLEAR HISTORY
const clearHistory = () => {
    commandHistory.value = [];
    localStorage.removeItem('commandHistory');
    toast.add({
        severity: 'success',
        summary: 'Cleared',
        detail: 'Command history cleared',
        life: 2000
    });
};

// 🔹 SAVE HISTORY
const saveHistory = () => {
    try {
        localStorage.setItem('commandHistory', JSON.stringify(commandHistory.value));
    } catch (error) {
        console.error('Error saving command history:', error);
    }
};

// 🔹 SCROLL TO BOTTOM
const scrollToBottom = async () => {
    await nextTick();
    if (terminalOutput.value) {
        terminalOutput.value.scrollTop = terminalOutput.value.scrollHeight;
    }
};

// 🔹 GET HELP
const showHelp = () => {
    const helpEntry = {
        id: Date.now().toString(),
        command: 'help',
        timestamp: new Date(),
        output: `Available commands:\n${availableCommands.value.map(cmd =>
            `  ${cmd.padEnd(20)} - ${CommandService.getCommandHelp(cmd)}`
        ).join('\n')}`,
        success: true
    };

    commandHistory.value.push(helpEntry);
    scrollToBottom();
};

// 🔹 FORMAT TIMESTAMP
const formatTime = (date) => {
    return new Date(date).toLocaleTimeString('it-IT', {
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    });
};
</script>

<template>
    <div class="card">
        <div class="flex justify-between items-center mb-4">
            <div>
                <h2 class="text-2xl font-bold mb-2">Command Console</h2>
                <p class="text-muted-color">Execute commands to control the bot system</p>
                <div class="flex items-center gap-2 mt-2">
                    <Tag value="MOCK MODE" severity="warn" />
                    <span class="text-xs text-muted-color">Server-side execution not yet implemented</span>
                </div>
            </div>
            <div class="flex gap-2">
                <Button label="Help" icon="pi pi-question-circle" severity="info" text @click="showHelp" />
                <Button label="Clear" icon="pi pi-trash" severity="danger" text @click="clearHistory" />
            </div>
        </div>

        <!-- TERMINAL OUTPUT -->
        <div ref="terminalOutput"
            class="bg-gray-900 text-green-400 p-4 rounded-lg font-mono text-sm mb-4 overflow-y-auto"
            style="height: 500px; max-height: 70vh;">

            <!-- WELCOME MESSAGE -->
            <div v-if="commandHistory.length === 0" class="text-gray-500 italic">
                <p>Bot Dashboard Command Console v1.0</p>
                <p>Type 'help' to see available commands</p>
                <p class="mt-2">Ready for input...</p>
            </div>

            <!-- COMMAND HISTORY -->
            <div v-for="entry in commandHistory" :key="entry.id" class="mb-3">
                <div class="flex items-center gap-2 text-blue-400">
                    <span class="text-gray-500">[{{ formatTime(entry.timestamp) }}]</span>
                    <span class="text-yellow-400">$</span>
                    <span>{{ entry.command }}</span>
                </div>
                <div :class="[
                    'mt-1 pl-4 whitespace-pre-wrap',
                    entry.success ? 'text-green-400' : 'text-red-400'
                ]">
                    {{ entry.output }}
                </div>
            </div>

            <!-- CURRENT INPUT PREVIEW -->
            <div v-if="executing" class="flex items-center gap-2 text-yellow-400">
                <i class="pi pi-spin pi-spinner"></i>
                <span>Processing command...</span>
            </div>
        </div>

        <!-- COMMAND INPUT -->
        <div class="relative">
            <div class="flex gap-2">
                <IconField class="flex-1">
                    <InputIcon class="pi pi-chevron-right text-green-500" />
                    <InputText v-model="commandInput" @keydown.enter="executeCommand" @keydown="handleKeyDown"
                        @input="onInputChange" placeholder="Type a command... (Tab for autocomplete, ↑↓ for history)"
                        :disabled="executing" class="font-mono" fluid />
                </IconField>
                <Button label="Execute" icon="pi pi-play" @click="executeCommand" :loading="executing"
                    :disabled="!commandInput.trim() || executing" />
            </div>

            <!-- AUTOCOMPLETE SUGGESTIONS -->
            <div v-if="showSuggestions"
                class="absolute top-full left-0 right-0 mt-1 bg-surface-0 dark:bg-surface-800 border border-surface-200 dark:border-surface-700 rounded-lg shadow-lg z-10 max-h-48 overflow-y-auto">
                <div v-for="cmd in filteredCommands" :key="cmd" @click="selectSuggestion(cmd)"
                    class="px-4 py-2 hover:bg-surface-100 dark:hover:bg-surface-700 cursor-pointer font-mono text-sm">
                    <div class="flex justify-between items-center">
                        <span class="font-semibold">{{ cmd }}</span>
                        <span class="text-xs text-muted-color">Press Tab</span>
                    </div>
                    <div class="text-xs text-muted-color mt-1">
                        {{ CommandService.getCommandHelp(cmd).split('.')[0] }}
                    </div>
                </div>
            </div>
        </div>

        <!-- QUICK COMMANDS -->
        <div class="mt-4 pt-4 border-t border-surface-200 dark:border-surface-700">
            <div class="text-sm font-semibold mb-2 text-muted-color">Quick Commands:</div>
            <div class="flex flex-wrap gap-2">
                <Button v-for="cmd in ['status', 'get-stats', 'list-bots', 'reload-config']" :key="cmd" :label="cmd"
                    size="small" severity="secondary" outlined @click="commandInput = cmd; executeCommand()"
                    :disabled="executing" />
            </div>
        </div>
    </div>
</template>

<style scoped>
/* Custom scrollbar for terminal */
.overflow-y-auto::-webkit-scrollbar {
    width: 8px;
}

.overflow-y-auto::-webkit-scrollbar-track {
    background: rgba(0, 0, 0, 0.2);
}

.overflow-y-auto::-webkit-scrollbar-thumb {
    background: rgba(34, 197, 94, 0.5);
    border-radius: 4px;
}

.overflow-y-auto::-webkit-scrollbar-thumb:hover {
    background: rgba(34, 197, 94, 0.7);
}
</style>
