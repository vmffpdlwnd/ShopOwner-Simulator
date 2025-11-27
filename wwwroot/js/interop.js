window.ShopOwnerSimulator = {
    copyToClipboard: async (text) => {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (err) {
            console.error('Failed to copy:', err);
            return false;
        }
    },
    
    getLocalStorage: (key) => {
        return localStorage.getItem(key);
    },
    
    setLocalStorage: (key, value) => {
        localStorage.setItem(key, value);
    },
    
    removeLocalStorage: (key) => {
        localStorage.removeItem(key);
    },
    
    clearLocalStorage: () => {
        localStorage.clear();
    },

    getCurrentTime: () => {
        return new Date().toISOString();
    },

    playSound: (soundName) => {
        // Implement sound effects if needed
        console.log('Sound:', soundName);
    }
};