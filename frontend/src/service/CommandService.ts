import { signalRService } from '@/api/signalRService';

export interface CommandRequest {
    command: string;
    parameters?: Record<string, any>;
}

export interface CommandResponse {
    success: boolean;
    output: string;
    timestamp: Date;
    executionTime?: number;
}

export interface CommandHistory {
    id: string;
    command: string;
    timestamp: Date;
    output: string;
    success: boolean;
}

export const CommandService = {
    // 🔹 EXECUTE COMMAND via SignalR
    async executeCommand(command: string, parameters?: Record<string, any>): Promise<CommandResponse> {
        try {
            const startTime = Date.now();
            
            // Check if SignalR is connected
            if (!signalRService.isConnected()) {
                try {
                    await signalRService.startConnection('/dashboardHub');
                } catch (connectError) {
                    return {
                        success: false,
                        output: 'Failed to connect to server. Please ensure you are logged in and the server is running.',
                        timestamp: new Date()
                    };
                }
            }

            // Execute command via SignalR
            const result = await signalRService.invoke('ExecuteCommand', {
                command,
                parameters
            });
            
            const executionTime = Date.now() - startTime;
            
            return {
                success: result?.success ?? true,
                output: result?.output || result?.message || 'Command executed successfully',
                timestamp: new Date(),
                executionTime
            };
        } catch (error: any) {
            return {
                success: false,
                output: error.message || 'Command execution failed',
                timestamp: new Date()
            };
        }
    },

    // 🔹 GET AVAILABLE COMMANDS (cached from backend if available)
    getAvailableCommands(): string[] {
        // Basic commands that should be available
        return [
            'help',
            'status',
            'list-bots',
            'start',
            'stop',
            'restart',
            'get-stats',
            'clear-cache',
            'reload-config',
            'bot-info',
            'enable-bot',
            'disable-bot'
        ];
    },

    // 🔹 GET COMMAND HELP
    getCommandHelp(command: string): string {
        const helpMap: Record<string, string> = {
            'help': 'Show available commands. Usage: help',
            'status': 'Get current bot service status. Usage: status',
            'list-bots': 'List all registered bots. Usage: list-bots',
            'start': 'Start the bot service. Usage: start',
            'stop': 'Stop the bot service. Usage: stop',
            'restart': 'Restart the bot service. Usage: restart',
            'get-stats': 'Retrieve current statistics. Usage: get-stats',
            'clear-cache': 'Clear application cache. Usage: clear-cache',
            'reload-config': 'Reload configuration from database. Usage: reload-config',
            'bot-info': 'Get information about a specific bot. Usage: bot-info <bot-id>',
            'enable-bot': 'Enable a specific bot. Usage: enable-bot <bot-id>',
            'disable-bot': 'Disable a specific bot. Usage: disable-bot <bot-id>'
        };
        
        return helpMap[command] || 'No help available for this command';
    }
};
