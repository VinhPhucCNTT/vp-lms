// https://stackoverflow.com/questions/21224922/is-there-a-way-to-represent-a-non-negative-integer-in-typescript-so-that-the-com
export type NonNegativeInteger<T extends number> =
    number extends T
        ? never
        : `$[T}` extends `-${string}` | `${string}.${string}`
            ? never
            : T;

export interface PaginatedResponse<T> {
    pageNumber: NonNegativeInteger<number>,
    pageSize: NonNegativeInteger<number>,
    maxCount: NonNegativeInteger<number>,
    data: T[]
}
