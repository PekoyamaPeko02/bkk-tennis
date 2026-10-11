declare namespace NodeJS {
  interface ProcessEnv {
    NODE_ENV: string;
    VUE_ROUTER_MODE: 'hash' | 'history' | 'abstract' | undefined;
    VUE_ROUTER_BASE: string | undefined;
    API_BASE: string;
    API_MOCK: string;
    APP_NAME: string;
  }
}

// Vite turns an imported image into its URL.
declare module '*.jpg' {
  const url: string;
  export default url;
}
