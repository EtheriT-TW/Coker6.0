const linkTitleTexts = {
    'zh-tw': {
        phone: '撥打電話：', email: '寄送郵件至：',
        address: '在 Google 地圖搜尋：', link: '連結至：',
        newWindow: '(另開新視窗)'
    },
    en: {
        phone: 'Call: ', email: 'Email to: ',
        address: 'Search on Google Maps: ', link: 'Link to: ',
        newWindow: ' (opens in a new window)'
    }
};

export function getLinkTitleTexts(locale) {
    const key = String(locale || '').trim().toLowerCase().replace('_', '-');
    return linkTitleTexts[key]
        || linkTitleTexts[key.split('-')[0]]
        || linkTitleTexts['zh-tw'];
}