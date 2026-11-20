import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

import {
    courseApi,
    courseCreateSchema,
    CourseSetResponse,
    type CourseCreateRequest,
} from "@/features/courses/api/course-api";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import {
    Field,
    FieldError,
    FieldGroup,
    FieldLabel,
} from "@/components/ui/field";
import { Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { PlusIcon } from "lucide-react";

interface CourseCreateDialogProps {
    onCreating: () => void;
    onCreated: (response: CourseSetResponse) => void;
}

export function CourseCreateDialog({ onCreating, onCreated }: CourseCreateDialogProps) {
    const form = useForm<CourseCreateRequest>({
        resolver: zodResolver(courseCreateSchema),
        defaultValues: {
            title: "",
            description: null,
            enrollmentOpen: false,
            backgroundFile: null,
        },
    });

    async function onSubmit(data: CourseCreateRequest) {
        console.log("Submitted: ", data);
        try {
            onCreating();
            const response = await courseApi.createCourse(data);
            form.reset();
            onCreated(response);
        } catch (error) {
            console.error("Failed to create course:", error);
        }
    }

    return (
        <Dialog>
            <DialogTrigger asChild>
                <Button variant="outline">
                    <PlusIcon className="size-4 mr-2" />
                    Create course
                </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-sm">
                <form
                    onSubmit={form.handleSubmit(onSubmit)}
                    className="space-y-6"
                >
                    <DialogHeader>
                        <DialogTitle>Create a new course</DialogTitle>
                        <DialogDescription>
                            Please enter the required information
                        </DialogDescription>
                    </DialogHeader>
                    <FieldGroup>
                        <Controller
                            name="title"
                            control={form.control}
                            render={({ field, fieldState }) => (
                                <Field data-invalid={fieldState.invalid}>
                                    <FieldLabel htmlFor="title">Title</FieldLabel>
                                    <Input
                                        {...field}
                                        id="title"
                                        placeholder="Course title"
                                        aria-invalid={fieldState.invalid}
                                    />
                                    {fieldState.invalid && (
                                        <FieldError errors={[fieldState.error]} />
                                    )}
                                </Field>
                            )}
                        />

                        <Controller
                            name="description"
                            control={form.control}
                            render={({ field, fieldState }) => (
                                <Field data-invalid={fieldState.invalid}>
                                    <FieldLabel htmlFor="description">
                                        Description
                                    </FieldLabel>
                                    <Textarea
                                        {...field}
                                        value={field.value ?? ""}
                                        onChange={(e) =>
                                            field.onChange(e.target.value || null)
                                        }
                                        id="description"
                                        placeholder="Course description..."
                                        aria-invalid={fieldState.invalid}
                                    />
                                    {fieldState.invalid && (
                                        <FieldError errors={[fieldState.error]} />
                                    )}
                                </Field>
                            )}
                        />

                        <Controller
                            name="enrollmentOpen"
                            control={form.control}
                            render={({ field, fieldState }) => (
                                <Field
                                    orientation="horizontal"
                                    data-invalid={fieldState.invalid}
                                >
                                    <FieldLabel htmlFor="enrollmentOpen">
                                        Enrollment Open
                                    </FieldLabel>
                                    <Switch
                                        id="enrollmentOpen"
                                        checked={field.value}
                                        onCheckedChange={field.onChange}
                                        aria-invalid={fieldState.invalid}
                                    />
                                    {fieldState.invalid && (
                                        <FieldError errors={[fieldState.error]} />
                                    )}
                                </Field>
                            )}
                        />

                        <Controller
                            name="backgroundFile"
                            control={form.control}
                            render={({ field, fieldState }) => (
                                <Field data-invalid={fieldState.invalid}>
                                    <FieldLabel htmlFor="backgroundFile">
                                        Background Image <span className="text-grey-50">(optional)</span>
                                    </FieldLabel>
                                    <Input
                                        id="backgroundFile"
                                        type="file"
                                        accept="image/png,image/jpeg,image/svg+xml"
                                        onChange={(e) =>
                                            field.onChange(e.target.files?.[0] ?? null)
                                        }
                                        aria-invalid={fieldState.invalid}
                                    />
                                    {fieldState.invalid && (
                                        <FieldError errors={[fieldState.error]} />
                                    )}
                                </Field>
                            )}
                        />
                    </FieldGroup>

                    <DialogFooter>
                        <DialogClose asChild>
                            <Button variant="outline">Cancel</Button>
                        </DialogClose>
                        <Button
                            type="submit"
                            disabled={form.formState.isSubmitting}
                        >
                            {form.formState.isSubmitting
                                ? "Creating..."
                                : "Create Course"}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog >
    );
}
