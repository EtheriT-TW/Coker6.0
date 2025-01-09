class LocalDb {
    #apiUrl;
    #dbName;
    #storeName;
    #lastInsertTime = null;
    #isReady = false;
    #readyResolve;

    constructor({ apiUrl, dbName, storeName }) {
        this.#apiUrl = apiUrl;
        this.#dbName = dbName;
        this.#storeName = storeName;

        // 初始化時建立 ready Promise，直到資料載入完成再 resolve
        this.ready = new Promise((resolve) => {
            this.#readyResolve = resolve;
        });

        // 開始檢查並更新資料
        this.checkAndFetchData();
    }

    async checkAndFetchData() {
        const lastInsertItem = (await this.getLastInsertTimeFromDB());
        const lastUpdateItem = (await this.getLastUpdateTimeFromDB());
        if (lastUpdateItem && this.isToday(lastUpdateItem.lastUpdateTime)) {
            console.log("資料已是今日最新，無需更新。");
            this.#lastInsertTime = lastInsertItem.lastInsertTime;
        } else {
            const apiResponse = await this.fetchDataFromApi(lastInsertItem.lastInsertTime);
            if (this.shouldUpdateData(apiResponse.lastInsertTime)) {
                await this.updateLocalData(apiResponse);
            }

            this.#lastInsertTime = apiResponse.lastInsertTime;
        }

        this.#isReady = true;
        this.#readyResolve(); // 資料載入完成，resolve ready Promise
    }

    async fetchDataFromApi(lastInsertTime) {
        const response = await fetch(this.#apiUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ LastInsertTime: lastInsertTime }),
        });

        if (!response.ok) throw new Error("Failed to fetch data");

        return response.json();
    }

    shouldUpdateData(newLastInsertTime) {
        return newLastInsertTime !== this.#lastInsertTime;
    }

    async updateLocalData(apiResponse) {
        const db = await this.openDatabase();

        const transaction = db.transaction([this.#storeName], "readwrite");
        const store = transaction.objectStore(this.#storeName);

        // 儲存新資料
        for (const item of apiResponse.keys) {
            store.put({ Key: item.key });
        }
        // 儲存新的 lastInsertTime
        store.put({ Key: "lastInsertTime", lastInsertTime: apiResponse.lastInsertTime });
        store.put({ Key: "lastUpdateTime", lastUpdateTime: new Date()});

        return new Promise((resolve, reject) => {
            transaction.oncomplete = () => resolve();
            transaction.onerror = (e) => reject(e);
        });
    }

    async openDatabase() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.#dbName, 1);

            request.onupgradeneeded = (e) => {
                const db = e.target.result;
                if (!db.objectStoreNames.contains(this.#storeName)) {
                    db.createObjectStore(this.#storeName, { keyPath: "Key" });
                }
            };

            request.onsuccess = (e) => resolve(e.target.result);
            request.onerror = (e) => reject(e);
        });
    }

    async getLastInsertTimeFromDB() {
        const db = await this.openDatabase();
        const transaction = db.transaction([this.#storeName], "readonly");
        const store = transaction.objectStore(this.#storeName);

        return new Promise((resolve, reject) => {
            const request = store.get("lastInsertTime");

            request.onsuccess = (e) => {
                const lastInsertTime = e.target.result;
                resolve(lastInsertTime || null);
            };
            request.onerror = (e) => reject(e);
        });
    }

    async getLastUpdateTimeFromDB() {
        const db = await this.openDatabase();
        const transaction = db.transaction([this.#storeName], "readonly");
        const store = transaction.objectStore(this.#storeName);

        return new Promise((resolve, reject) => {
            const request = store.get("lastUpdateTime");

            request.onsuccess = (e) => {
                const lastUpdateTime = e.target.result;
                resolve(lastUpdateTime || null);
            };
            request.onerror = (e) => reject(e);
        });
    }

    // 判斷日期是否為當天
    isToday(date) {
        const today = new Date();
        const inputDate = new Date(date);
        return today.getFullYear() === inputDate.getFullYear() &&
            today.getMonth() === inputDate.getMonth() &&
            today.getDate() === inputDate.getDate();
    }

    // 新增公開方法用於取得資料
    async getData() {
        if (!this.#isReady) {
            await this.ready; // 等待資料載入完成
        }

        const db = await this.openDatabase();
        const transaction = db.transaction([this.#storeName], "readonly");
        const store = transaction.objectStore(this.#storeName);

        return new Promise((resolve, reject) => {
            const request = store.getAll();

            request.onsuccess = (e) => resolve(e.target.result);
            request.onerror = (e) => reject(e);
        });
    }
}
