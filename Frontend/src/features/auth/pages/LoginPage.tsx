import {
    Anchor,
    Button,
    Checkbox,
    Container,
    Group,
    Paper,
    PasswordInput,
    Text,
    TextInput,
    Title,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import classes from '../styles/LoginPage.module.css';

export default function LoginPage() {
    const formState = useForm({
        initialValues: {
            email: "",
            password: "",
            remember: false
        },
        validate: {
            email: (val: string) => (/^\S+@\S+$/.test(val) ? null : 'Invalid email'),
            password: (val: string) => (val.length < 6 ? 'Password should include at least 6 characters' : null),
        },
        transformValues: (values) => ({
            email: values.email,
            password: values.password
        })
    });

    return (
        <Container size={420} my={40}>
            <Title ta="center" className={classes.title}>
                Welcome back!
            </Title>

            <Text className={classes.subtitle}>
                Do not have an account yet? <Anchor href='/register'>Create account</Anchor>
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
                    <Group justify="space-between" mt="lg">
                        <Checkbox
                            label="Remember me" 
                            checked={formState.values.remember}
                            onChange={(e) => formState.setFieldValue("remember", e.currentTarget.checked)}
                        />
                        <Anchor component="button" size="sm">
                            Forgot password?
                        </Anchor>
                    </Group>
                    <Button type="submit" fullWidth mt="xl" radius="md">
                        Sign in
                    </Button>
                </form>
            </Paper>
        </Container>
    );
}
