import { z } from "zod";

export const sqidsAlphabetRegex = /^[a-zA-Z0-9\-_]*$/g;
export const idSchema = z.string().min(5)
    .refine(
        (id) => sqidsAlphabetRegex.test(id),
        { message: "Invalid sqids id format." }
    );

const allowedImageTypes = ["image/png", "image/jpeg", "image/jpg", "image/svg+xml"];
export const imageFileSchema = z.instanceof(File)
    .refine(
        (file) => allowedImageTypes.includes(file.type),
        { message: "Invalid image file type." }
    );

export const pageRequestSchema = z.object({
    pageNumber: z.number().int().min(1),
    pageSize: z.number().int().min(1)
});
export type PageRequest = z.infer<typeof pageRequestSchema>;
