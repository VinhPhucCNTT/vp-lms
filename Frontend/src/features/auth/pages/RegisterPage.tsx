import {
    Anchor,
    Button,
    Checkbox,
    Container,
    Paper,
    Group,
    PasswordInput,
    Text,
    TextInput,
    Title,
} from '@mantine/core';
import { matchesField, useForm } from '@mantine/form';
import classes from '../styles/RegisterPage.module.css';

export default function RegisterPage() {
    const formState = useForm({
        initialValues: {
            email: "",
            fullName: "",
            userName: "",
            password: "",
            passConfirm: "",
            terms: false
        },
        validate: {
            email: (val: string) => (/^\S+@\S+$/.test(val) ? null : 'Invalid email'),
            password: (val: string) => (val.length < 6 ? 'Password should include at least 6 characters' : null),
            passConfirm: matchesField("password", "Passwords do not match"),
            terms: (val) => (!val ? "You must agree to the terms of service to continue" : null)
        },
        transformValues: (values) => ({
            email: values.email,
            fullName: values.fullName,
            userName: values.userName,
            password: values.password
        }),
    });

    return (
        <Container size={420} my={40}>
            <Title ta="center" className={classes.title}>
                Welcome to vp-lms!
            </Title>

            <Text className={classes.subtitle}>
                Already have an account? <Anchor href='/login'>Login here</Anchor>
            </Text>

            <Paper withBorder shadow="sm" p={22} mt={30} radius="md">
                <form onSubmit={formState.onSubmit((v) => { console.log(v) })}>
                    <TextInput
                        required
                        label="Email"
                        placeholder="you@email.com"
                        value={formState.values.email}
                        onChange={(e) => formState.setFieldValue("email", e.currentTarget.value)}
                        error={formState.errors.email && "Invalid email"}
                        radius="md"
                        key={formState.key("email")}
                    />
                    <PasswordInput
                        required
                        label="Password"
                        placeholder="Your password"
                        value={formState.values.password}
                        onChange={(e) => formState.setFieldValue("password", e.currentTarget.value)}
                        error={formState.errors.password && "Password must be at least 6 characters long"}
                        mt="md"
                        radius="md"
                        key={formState.key("password")}
                    />
                    <PasswordInput
                        required
                        label="Confirm password"
                        placeholder="Confirm password"
                        value={formState.values.passConfirm}
                        onChange={(e) => formState.setFieldValue("passConfirm", e.currentTarget.value)}
                        error={formState.errors.passConfirm && "Passwords do not match"}
                        mt="md"
                        radius="md"
                    />
                    <Group justify="start" mt="md">
                        <Checkbox
                            label="I agree to the terms and conditions"
                            checked={formState.values.terms}
                            onChange={(e) => formState.setFieldValue("terms", e.currentTarget.checked)}
                            error={formState.errors.terms && "You must agree to the terms of service to continue"}
                        />
                    </Group>
                    <Button type="submit" fullWidth mt="xl" radius="md">
                        Create account
                    </Button>
                </form>
            </Paper>
        </Container>
    );
}
