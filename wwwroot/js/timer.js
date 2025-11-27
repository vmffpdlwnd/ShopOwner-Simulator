class TimerManager {
    constructor() {
        this.timers = new Map();
    }

    start(timerId, endTime, callback) {
        const interval = setInterval(() => {
            const now = new Date().getTime();
            const remaining = new Date(endTime).getTime() - now;

            if (remaining <= 0) {
                clearInterval(interval);
                this.timers.delete(timerId);
                if (callback) callback();
            }
        }, 1000);

        this.timers.set(timerId, interval);
    }

    stop(timerId) {
        if (this.timers.has(timerId)) {
            clearInterval(this.timers.get(timerId));
            this.timers.delete(timerId);
        }
    }

    stopAll() {
        this.timers.forEach(interval => clearInterval(interval));
        this.timers.clear();
    }
}

window.TimerManager = new TimerManager();