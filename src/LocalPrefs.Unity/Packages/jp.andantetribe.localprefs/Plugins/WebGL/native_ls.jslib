mergeInto(LibraryManager.library, {

    SaveToLocalStorage: function (keyStr, valueStr){
        const key = UTF8ToString(keyStr);
        const value = UTF8ToString(valueStr);
        localStorage.setItem(key, value);
    },
    
    DeleteFromLocalStorage: function (keyStr) {
        const key = UTF8ToString(keyStr);
        localStorage.removeItem(key);
    },
    
    LoadFromLocalStorage: function (keyStr, success) {
        if (typeof localStorage === 'undefined' && window.localStorage == null){
            window.alert("LocalStorage is not supported in this environment.");
            return null;
        }
        const key = UTF8ToString(keyStr);
        const value = localStorage.getItem(key);
        if (value !== null) {
            const buffer = stringToNewUTF8(value);
            return buffer;
        } else {
            return null;
        }
    },
});