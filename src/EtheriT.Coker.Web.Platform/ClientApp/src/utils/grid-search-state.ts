import { onBeforeRouteLeave } from "vue-router";
import type dxDataGrid from "devextreme/ui/data_grid";
import type { InitializedEvent } from "devextreme/ui/data_grid";

/**
 * DataGrid 搜尋框停止打字 0.7 秒後才套用並存檔，這段時間內離開頁面，
 * 剛打的字不會寫進 sessionStorage。離開清單前把搜尋框目前的文字補寫進存檔。
 * 回傳的函式綁到 DxDataGrid 的 @initialized。
 */
export function useGridSearchPersistence(storageKey: string): (e: InitializedEvent) => void {
    let grid: dxDataGrid | undefined;

    onBeforeRouteLeave(() => {
        const input = grid?.element().querySelector<HTMLInputElement>(".dx-datagrid-search-panel input");
        if (!grid || !input) return;

        const state = grid.state() as { searchText?: string };
        if ((state.searchText ?? "") === input.value) return;

        // 搜尋字變了，頁碼跟 DataGrid 自己搜尋時一樣回到第一頁
        sessionStorage.setItem(storageKey, JSON.stringify({ ...state, searchText: input.value, pageIndex: 0 }));
    });

    return e => {
        grid = e.component;
    };
}
