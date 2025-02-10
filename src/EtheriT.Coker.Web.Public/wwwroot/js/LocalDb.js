class LocalDb {
    #apiUrl;
    #dbName;
    #storeName;
    #lastInsertTime = null;
    #isReady = false;
    #readyResolve;
    #db;

    constructor({ apiUrl, dbName, storeName }) {
        this.#apiUrl = apiUrl;
        this.#dbName = dbName;
        this.#storeName = storeName;

        // 初始化時建立 ready Promise，直到資料載入完成再 resolve
        this.ready = new Promise((resolve) => {
            this.#readyResolve = resolve;
        });

        // 開始檢查並更新資料
        this.#initializeDatabase();
    }
    async #initializeDatabase() {
        try {
            const dbExists = await this.#checkDatabaseExists();
            if (!dbExists) {
                console.warn("資料庫不存在，刪除舊版本並重新建立...");
                await this.#deleteDatabase(); // 使用私有方法刪除舊資料庫
            } else {
                const isKeyPathValid = await this.#checkKeyPath();
                if (!isKeyPathValid) {
                    console.warn("資料庫 keyPath 不正確，刪除並重新建立資料庫。");
                    await this.#deleteDatabase(); // 使用私有方法刪除舊資料庫
                }
            }

            
            await this.checkAndFetchData();
        } catch (error) {
            console.error("初始化資料庫時發生錯誤:", error);
        }
    }

    async #checkDatabaseExists() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.#dbName);
            request.onsuccess = (event) => {
                const db = event.target.result;
                const storeNames = Array.from(db.objectStoreNames);
                db.close();
                resolve(storeNames.includes(this.#storeName)); // 檢查是否包含目標 store
            };
            request.onerror = (event) => {
                console.error("檢查資料庫存在時發生錯誤:", event.target.error);
                reject(event.target.error);
            };
            request.onupgradeneeded = () => {
                // 如果 onupgradeneeded 被觸發，代表資料庫不存在或需要升級
                resolve(false);
            };
        });
    }

    async #checkKeyPath() {
        return new Promise((resolve) => {
            const request = indexedDB.open(this.#dbName);

            request.onsuccess = (event) => {
                const db = event.target.result;
                const store = db.transaction(this.#storeName, "readonly").objectStore(this.#storeName);

                // 檢查 keyPath
                resolve(store.keyPath === "compositeKey");
                db.close();
            };

            request.onerror = () => resolve(false);
        });
    }

    async #deleteDatabase() {
        return new Promise((resolve, reject) => {
            const deleteRequest = indexedDB.deleteDatabase(this.#dbName);
            deleteRequest.onsuccess = () => {
                console.log(`資料庫 ${this.#dbName} 已刪除`);
                resolve();
            };
            deleteRequest.onerror = (event) => {
                console.error("刪除資料庫時發生錯誤:", event.target.error);
                reject(event.target.error);
            };
            deleteRequest.onblocked = () => {
                console.warn("刪除資料庫被阻塞。請關閉其他使用此資料庫的連線。");
            };
        });
    }

    async checkAndFetchData() {
        const lastInsertItem = (await this.getLastInsertTimeFromDB());
        const lastUpdateItem = (await this.getLastUpdateTimeFromDB());
        if (lastUpdateItem && this.isToday(lastUpdateItem.lastUpdateTime)) {
            console.log("資料已是今日最新，無需更新。");
            this.#lastInsertTime = lastInsertItem.lastInsertTime;
            this.deleteData("UNDEFINED","remote");
        } else {
            const apiResponse = await this.fetchDataFromApi(lastInsertItem == null ? lastInsertItem : lastInsertItem.lastInsertTime);
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

        return new Promise((resolve, reject) => {
            apiResponse.keys.forEach((item) => {
                item.type = "remote";
                const compositeKey = `${item.key}|${item.type}`;
                const getRequest = store.get(compositeKey);

                getRequest.onsuccess = (e) => {
                    const existingItem = e.target.result;

                    if (existingItem) {
                        existingItem.times += item.times || 0;
                        store.put(existingItem);
                    } else {
                        store.put({
                            compositeKey,
                            key: item.key,
                            type: item.type,
                            times: item.times || 0,
                        });
                    }
                };

                getRequest.onerror = (err) => reject(err);
            });

            store.put({ compositeKey: "lastInsertTime", lastInsertTime: apiResponse.lastInsertTime });
            store.put({ compositeKey: "lastUpdateTime", lastUpdateTime: new Date() });

            transaction.oncomplete = () => resolve();
            transaction.onerror = (e) => reject(e);
        });
    }

    async openDatabase() {
        if (this.#db) return this.#db;
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.#dbName, 1);

            request.onupgradeneeded = (e) => {
                const db = e.target.result;
                if (!db.objectStoreNames.contains(this.#storeName)) {
                    db.createObjectStore(this.#storeName, { keyPath: "compositeKey" });
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

            request.onsuccess = (e) => {
                const allData = e.target.result;
                // 過濾掉系統資料
                const filteredData = allData
                    .filter(item => item.compositeKey !== "lastUpdateTime" && item.compositeKey !== "lastInsertTime" && item.key.toLowerCase() != "undefined")
                    .map(item => ({
                        key: item.key,
                        type: item.type,
                        times: item.times,
                    }));
                // 依據 times 欄位排序
                const sortedData = filteredData.sort((a, b) => b.times - a.times);

                resolve(sortedData);
            };

            request.onerror = (e) => reject(e);
        });
    }

    async addOrUpdateData(key, type = "local") {
        if (!this.#isReady) {
            await this.ready; // 等待資料載入完成
        }

        const db = await this.openDatabase();
        const transaction = db.transaction([this.#storeName], "readwrite");
        const store = transaction.objectStore(this.#storeName);

        return new Promise((resolve, reject) => {
            const compositeKey = `${key}|${type}`;

            // 嘗試取得現有資料
            const getRequest = store.get(compositeKey);

            getRequest.onsuccess = (e) => {
                const existingItem = e.target.result;

                if (existingItem) {
                    // 如果資料已存在，更新 times 值並儲存
                    existingItem.times++;
                    store.put(existingItem);
                } else {
                    // 如果資料不存在，新增資料
                    store.put({
                        compositeKey,
                        key,
                        type,
                        times: 1,
                    });
                }

                transaction.oncomplete = () => resolve(true);
                transaction.onerror = (err) => reject(err);
            };

            getRequest.onerror = (err) => reject(err);
        });
    }

    async deleteData(key, type = "local") {
        const compositeKey = `${key}|${type}`; // 在方法內組合 compositeKey
        const db = await this.openDatabase();
        const transaction = db.transaction([this.#storeName], "readwrite");
        const store = transaction.objectStore(this.#storeName);

        return new Promise((resolve, reject) => {
            const request = store.delete(compositeKey); // 使用 compositeKey 執行刪除

            request.onsuccess = () => {
                if (type === "local") console.log(`成功刪除本機資料: ${compositeKey}`);
                resolve();
            };

            request.onerror = (error) => {
                console.error(`刪除失敗: ${compositeKey}`, error);
                reject(error);
            };
        });
    }

    async clearLocalData() {
        try {
            const db = await this.openDatabase();
            const transaction = db.transaction([this.#storeName], "readwrite");
            const store = transaction.objectStore(this.#storeName);

            return new Promise((resolve, reject) => {
                const request = store.openCursor();
                const keysToDelete = [];

                request.onsuccess = (event) => {
                    const cursor = event.target.result;
                    if (cursor) {
                        const key = cursor.key;
                        if (key.endsWith("|local")) {
                            keysToDelete.push(key);
                        }
                        cursor.continue(); // 繼續遍歷
                    } else {
                        // 開始刪除記錄
                        keysToDelete.forEach((key) => {
                            store.delete(key);
                        });
                        console.log("成功清空本地紀錄");
                        resolve();
                    }
                };

                request.onerror = (error) => {
                    console.error("清空本地紀錄時發生錯誤:", error);
                    reject(error);
                };
            });
        } catch (error) {
            console.error("清空本地資料時發生錯誤:", error);
            throw error;
        }
    }

}
