import {
  QueryClient,
  defaultShouldDehydrateQuery,
} from "@tanstack/react-query";

export function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30 * 1000,
      },
      dehydrate: {
        // Поведение по умолчанию для Next.js SSR
        shouldDehydrateQuery: (query) =>
          defaultShouldDehydrateQuery(query) ||
          query.state.status === "pending",
      },
    },
  });
}

let browserQueryClient: QueryClient | undefined = undefined;

export function getQueryClient() {
  if (typeof window === "undefined") {
    // На сервере: всегда создаем новый клиент для каждого запроса
    return makeQueryClient();
  } else {
    // На клиенте: создаем один раз и переиспользуем в браузере
    if (!browserQueryClient) browserQueryClient = makeQueryClient();
    return browserQueryClient;
  }
}
