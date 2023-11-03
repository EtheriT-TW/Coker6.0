const traitInputAttr = { placeholder: '例如. 輸入文字' };

let tw = {
    assetManager: {
        addButton: '新增圖片',
        inputPlh: 'http://path/to/the/image.jpg',
        modalTitle: '選擇圖片',
        uploadTitle: '點擊或拖曳圖片上傳',
    },
    domComponents: {
        names: {
            '': 'Box',
            wrapper: 'Body',
            text: '文字',
            comment: '評論',
            image: '圖片',
            video: '影片',
            label: '文字',
            link: '超連結',
            map: '地圖',
            tfoot: '表尾',
            tbody: '表身',
            thead: '表頭',
            table: '表格',
            row: '行',
            cell: '儲存格',
        },
    },
    deviceManager: {
        device: '設備',
        devices: {
            desktop: '電腦',
            tablet: '平板',
            mobileLandscape: '手機橫放',
            mobilePortrait: '手機垂直',
        },
    },
    panels: {
        buttons: {
            titles: {
                preview: '預覽',
                fullscreen: '全螢幕',
                'sw-visibility': '查看組件',
                'export-template': '查看原始碼',
                'open-sm': '打開樣式管理氣',
                'open-tm': '設定',
                'open-layers': '打開布局管理氣',
                'open-blocks': '打開區塊',
            },
        },
    },
    selectorManager: {
        label: 'Classes',
        selected: 'Selected',
        emptyState: '- State -',
        states: {
            hover: 'Hover',
            active: 'Click',
            'nth-of-type(2n)': 'Even/Odd',
        },
    },
    styleManager: {
        empty: '設定樣式前，請選擇一個元素',
        layer: '階層',
        fileButton: '圖片',
        sectors: {
            general: '常見',
            layout: '排版',
            typography: '版面',
            decorations: '設計',
            extra: '擴展',
            flex: '模型',
            dimension: '尺寸',
        },
        // The core library generates the name by their `property` name
        properties: {
            // float: 'Float',
        },
    },
    traitManager: {
        empty: '設定前請選擇一個元素',
        label: '組件設定',
        traits: {
            // In a simple trait, like text input, these are used on input attributes
            attributes: {
                id: traitInputAttr,
                alt: traitInputAttr,
                title: traitInputAttr,
                href: { placeholder: 'eg. https://google.com' },
            },
            // In a trait like select, these are used to translate option names
            options: {
                target: {
                    false: '直接連結',
                    _blank: '另開新視窗',
                },
            },
        },
    },
};
